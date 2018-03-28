using PiTung.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static ShareMod.Remote;

namespace ShareMod
{
    internal class RUser
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
                    IGConsole.Log("Dont cache user " + id);
                    return null;
                }

                UserCache[id] = user;
            }

            return UserCache[id];
        }

        public string ChangePassword(string newPassword, string confirm)
        {
            var r = MakeRequest<LoginModel>("/user/update/password", HttpMethod.Post, new Dictionary<string, object>
            {
                ["token"] = this.Token,
                ["password"] = newPassword,
                ["confirm"] = confirm
            });

            if (r.Status == "ok")
            {
                this.Token = r.Token;
                StoreEncrypted(TokenFilePath, r.Token);

                return "ok";
            }

            return r.Error;
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
}
