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

        public const string Endpoint = "https://www.pipe0481.heliohost.org/pitung/share/v1";

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

            public bool Login(string username, string password)
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

                return a.Error == null;
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

            public UserModel GetByID(int id)
            {
                if (!UserCache.ContainsKey(id))
                {
                    var user = MakeRequest<UserModel>("/user", HttpMethod.Get, new Dictionary<string, object>
                    {
                        ["id"] = id
                    });

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
            return MakeRequest<WorldsResponseModel>("/worlds", HttpMethod.Get, new Dictionary<string, object>
            {
                ["page"] = page
            });
        }

        public void DownloadWorld(int id, Action<byte[]> done)
        {
            var req = BuildRequest("/world", HttpMethod.Get, new Dictionary<string, object>
            {
                ["id"] = id,
                ["d"] = 1
            });

            var yie = req.SendWebRequest();

            new Thread(() =>
            {
                while (!yie.isDone)
                {
                    Thread.Sleep(50);
                }

                byte[] down = req.downloadHandler.data;


                if ((char)down[0] == '{')
                {
                    Console.WriteLine("WORLD NOT FOUND");
                    done(null);
                }
                else
                {
                    done(req.downloadHandler.data);
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
        private static UnityWebRequest BuildRequest(string api, HttpMethod method, Dictionary<string, object> data = null)
        {
            if (data != null)
                api += "?" + string.Join("&", data.Select(o => $"{o.Key}={UnityWebRequest.EscapeURL(o.Value.ToString())}").ToArray());

            UnityWebRequest req = null;

            switch (method)
            {
                case HttpMethod.Post:
                    req = UnityWebRequest.Post(Endpoint + api, "");
                    break;
                case HttpMethod.Get:
                    req = UnityWebRequest.Get(Endpoint + api);
                    break;
            }

            return req;
        }

        private static byte[] MakeBinaryRequest(string api, HttpMethod method, Dictionary<string, object> data = null)
        {
            var req = BuildRequest(api, method, data);

            ManualResetEventSlim mre = new ManualResetEventSlim();

            var yie = req.SendWebRequest();

            new Thread(() =>
            {
                while (!yie.isDone)
                {
                    Thread.Sleep(50);
                }

                mre.Set();
            }).Start();

            mre.Wait();

            return req.downloadHandler.data;
        }

        private static T MakeRequest<T>(string api, HttpMethod method, Dictionary<string, object> data = null) where T : Model
        {
            byte[] bin = MakeBinaryRequest(api, method, data);

            string str = Encoding.UTF8.GetString(bin);
            
            return SimpleJson.DeserializeObject<T>(str, new MySerializationStrategy());
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
