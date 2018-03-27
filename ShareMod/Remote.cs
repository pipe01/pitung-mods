using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web.Management;
using UnityEngine;
using UnityEngine.Networking;

namespace ShareMod
{
    internal class Remote
    {
        private enum HttpMethod
        {
            Post,
            Get
        }

#if DEBUG
        public const string Endpoint = "http://localhost/pitung/share/v1";
#elif RELEASE
        public const string Endpoint = "https://www.pipe0481.heliohost.org/pitung/share/v1";
#endif

        private static WebClient Client = new WebClient();
        private static Semaphore ClientUsage = new Semaphore(1, 1);

        public RUser User { get; } = new RUser();
        public class RUser
        {
            private const string IActuallyDo = "i have crippling depression";

            private static Dictionary<int, UserModel> UserCache = new Dictionary<int, UserModel>();

            public int UserID { get; private set; }
            public string Token { get; private set; }
            public bool IsLoggedIn => Token != null;

            private string TokenFilePath = Path.Combine(Application.persistentDataPath, $"sharemod{Path.DirectorySeparatorChar}token.bin");

            public bool TryLoginFromFile()
            {
                if (!File.Exists(TokenFilePath))
                    return false;

                byte[] file = File.ReadAllBytes(TokenFilePath);
                byte[] dec = Encrypt(file, IActuallyDo);

                this.Token = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(dec)));
                Console.WriteLine(this.Token);

                //TODO Call /user/check and get user id

                return true;
            }

            public string Login(string username, string password)
            {
                var a = MakeRequest<LoginModel>("/user/login", HttpMethod.Post, new Dictionary<string, object>
                {
                    ["username"] = username,
                    ["password"] = password
                });
                
                if (a.Error == null)
                {
                    this.Token = a.Token;
                    this.UserID = a.UserID;

                    StoreEncrypted(TokenFilePath, a.Token);
                }

                return a.Status ?? a.Error;
            }

            public bool Logout()
            {
                if (!IsLoggedIn)
                    return false;

                var r = MakeRequest<Model>("/user/logout", HttpMethod.Post, new Dictionary<string, object>
                {
                    ["token"] = this.Token
                });

                if (r.Status != "ok")
                    return false;

                this.Token = null;

                if (File.Exists(TokenFilePath))
                    File.Delete(TokenFilePath);

                return true;
            }

            public string Register(string username, string password)
            {
                var r = MakeRequest<Model>("/user/register", HttpMethod.Post, new Dictionary<string, object>
                {
                    ["username"] = username,
                    ["password"] = password
                });

                Console.WriteLine(r.Status ?? r.Error ?? "null response");

                return r.Status ?? r.Error;
            }

            public UserModel GetByID(int id)
            {
                if (!UserCache.ContainsKey(id))
                {
                    var user = MakeRequest<UserModel>($"/user/{id}", HttpMethod.Get);

                    if (user == null) //Error on the request, don't cache
                    {
                        return null;
                    }
                    
                    UserCache[id] = user;
                }

                return UserCache[id];
            }

            private static void StoreEncrypted(string filePath, string str)
            {
                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                byte[] data = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(str)));

                data = Encrypt(data, IActuallyDo);

                File.WriteAllBytes(filePath, data);
            }

            private static byte[] Encrypt(byte[] data, string key, int chunkSize = 16)
            {
                return Flip(XOR(Flip(data)));

                byte[] Flip(byte[] d)
                {
                    List<byte> file = new List<byte>();

                    int i = 0;
                    while (i < d.Length)
                    {
                        var chunk = d.Skip(i).Take(chunkSize);

                        file.AddRange(chunk.Reverse());

                        i += chunk.Count();
                    }

                    return file.ToArray();
                }

                byte[] XOR(byte[] d)
                {
                    byte[] ret = new byte[d.Length];

                    for (int i = 0; i < d.Length; i++)
                    {
                        int keyIndex = i % key.Length;

                        byte keyB = (byte)key[keyIndex];

                        ret[i] = (byte)(d[i] ^ keyB);
                    }

                    return ret;
                }
            }
        }
        
        public WorldsResponseModel GetWorlds(int page)
        {
            return MakeRequest<WorldsResponseModel>($"/worlds/{page}", HttpMethod.Get);
        }

        public void DownloadWorld(int id, Action<byte[]> done)
        {
            new Thread(() =>
            {
                var req = MakeRequest($"/world/{id}/d", HttpMethod.Get);
                
                if ((char)req[0] == '{')
                {
                    done(null);
                }
                else
                {
                    done(req);
                }
            }).Start();
        }

        public WorldModel UploadWorld(byte[] zipBytes, string worldName)
        {
            var client = new WebClient();

            NameValueCollection form = new NameValueCollection();
            form.Add("token", User.Token);
            form.Add("title", worldName);

            ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            File.WriteAllBytes("world", zipBytes);

            client.QueryString = form;

            try
            {
                byte[] result = client.UploadFile(Endpoint + "/world", "world");

                string resp = Encoding.UTF8.GetString(result);

                return SimpleJson.DeserializeObject<WorldModel>(resp, new MySerializationStrategy());
            }
            finally
            {
                File.Delete("world");
            }
        }
        
#region HTTP Stuff
        private static T MakeRequest<T>(string api, HttpMethod method, Dictionary<string, object> data = null) where T : Model
        {
            try
            {
                byte[] bin = MakeRequest(api, method, data);
                
                if (bin == null)
                {
                    return null;
                }

                string str = Encoding.UTF8.GetString(bin);

                try
                {
                    return SimpleJson.DeserializeObject<T>(str, new MySerializationStrategy());
                }
                catch (System.Runtime.Serialization.SerializationException)
                {
#if DEBUG
                    Console.WriteLine("At MakeRequest<T>:");
                    Console.Write("INVALID JSON STRING: " + api);
                    Console.WriteLine(str);
#endif

                    return (T)new Model { Error = "invalid response" };
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine("MakeRequest<T> exception: " + ex);
#endif
                return null;
            }

        }

        private static byte[] MakeRequest(string api, HttpMethod method, Dictionary<string, object> data = null)
        {
            ClientUsage.WaitOne();

            NameValueCollection form = new NameValueCollection();

            if (data != null)
            {
                foreach (var item in data)
                {
                    form.Add(item.Key, item.Value.ToString());
                }
            }

            ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            
            Client.QueryString = form;

#if DEBUG
            Console.WriteLine("BEGIN REQUEST TO " + api);
#endif

            byte[] response = null;

            try
            {
                if (method == HttpMethod.Get)
                {
                    response = Client.DownloadData(Endpoint + api);
                }
                else
                {
                    response = Client.UploadData(Endpoint + api, "POST", new byte[0]);
                }
            }
            catch (WebException ex)
            {
#if DEBUG
                Console.WriteLine("At MakeRequest: " + ex);
#endif

                if (ex.Response == null)

                {
                    return null;
                }

                string str = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

#if DEBUG
                Console.WriteLine("Response: " + str);
#endif

                response = Encoding.UTF8.GetBytes(str);

            }
            finally
            {
                ClientUsage.Release();

#if DEBUG
                Console.WriteLine("RESPONSE RECEIVED FOR " + api);
                Console.WriteLine("DATA: " + (response == null ? "NULL" : Encoding.UTF8.GetString(response)));
#endif
            }
            
            return response;
        }

        private class MySerializationStrategy : PocoJsonSerializerStrategy
        {
            protected override string MapClrMemberNameToJsonFieldName(string clrPropertyName)
            {
                return clrPropertyName.ToLower();
            }
        }
#endregion
    }
}
