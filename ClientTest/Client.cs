using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    public class Client
    {
        private TcpClient TClient;

        private NetworkStream _Stream;
        private NetworkStream Stream => TClient == null ? null : (_Stream ?? (_Stream = TClient?.GetStream()));

        private const int ServerPort = 5757;
        private const int BufferSize = 1024;

        public event EventHandler<string> DataReceived;

        public void Start()
        {
            TClient = new TcpClient("127.0.0.1", ServerPort);

            StartRead();
        }

        public void Stop()
        {
            TClient.Close();
            TClient = null;
        }

        public void Send(string str)
        {
            if (TClient == null || !TClient.Connected)
                return;

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            
            Stream.Write(bytes, 0, bytes.Length);
            Stream.Flush();
        }

        private void StartRead()
        {
            if (TClient?.Connected == true)
                new Thread(ReadStreamThread).Start();
        }

        private void ReadStreamThread()
        {
            var reader = new StreamReader(Stream, Encoding.UTF8);

            while (TClient.Connected)
            {
                if (TClient.Client.Available > 0)
                {
                    var builder = new StringBuilder();
                    var buffer = new byte[BufferSize];
                    int read;

                    do
                    {
                        read = Stream.Read(buffer, 0, buffer.Length);

                        builder.Append(Encoding.UTF8.GetString(buffer, 0, read).TrimEnd('\0'));
                    }
                    while (TClient.Client.Available > 0);

                    DataReceived?.Invoke(this, builder.ToString());
                }
            }
        }
    }
}
