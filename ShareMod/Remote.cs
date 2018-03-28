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
        public enum HttpMethod
        {
            Post,
            Get
        }

#if DEBUG
        public const string Endpoint = "http://localhost/pitung/share/v1";
#else
        public const string Endpoint = "https://www.pipe0481.heliohost.org/pitung/share/v1";
#endif

        private static WebClient Client = new WebClient();
        private static Semaphore ClientUsage = new Semaphore(1, 1);

        public RUser User { get; } = new RUser();
        
        
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
        public static T MakeRequest<T>(string api, HttpMethod method, Dictionary<string, object> data = null) where T : Model
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

        public static byte[] MakeRequest(string api, HttpMethod method, Dictionary<string, object> data = null)
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
