using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoMod_Server
{
    internal class Server
    {
        private TcpListener Listener;
        private TcpClient Remote;

        private const int ServerPort = 5757;
        private const int BufferSize = 1024;

        public delegate void DataReceivedDelegate(string data);
        public event DataReceivedDelegate DataReceived = delegate { };

        public event EventHandler<string> Log = delegate { };
        public event EventHandler<string> DebugLog = delegate { };

        private NetworkStream _Stream;
        private NetworkStream Stream => Remote == null ? null : (_Stream ?? (_Stream = Remote?.GetStream()));

        public Server()
        {
            Listener = new TcpListener(IPAddress.Loopback, ServerPort);
        }

        public void Start()
        {
            Listener.Start();

            AcceptClient();
        }

        private void AcceptClient()
        {
            DebugLog(this, "Accepting clients...");

            Listener.BeginAcceptTcpClient(AcceptTcpClient, null);

            void AcceptTcpClient(IAsyncResult ar)
            {
                DebugLog(this, "Client found.");

                Remote = Listener.EndAcceptTcpClient(ar);
                StartRead();

                DebugLog(this, "Connected to client.");

                new Thread(() =>
                {
                    Thread.Sleep(200);
                    DebugLog(this, "Sending hi...");
                    Send("hi");
                }).Start();
            }
        }

        public void Stop(bool acceptNewClient)
        {
            if (Remote == null)
                return;

            DebugLog(this, "Stopping.");

            Remote.Close();
            Remote = null;

            if (acceptNewClient)
                AcceptClient();
        }
        
        public void Send(string str)
        {
            if (Remote == null)
                return;

            if (str[str.Length - 1] != '\n')
                str = str + "\n";

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            var stream = Remote.GetStream();

            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        private void StartRead()
        {
            if (Remote?.Connected == true)
                new Thread(ReadStreamThread).Start();

            void ReadStreamThread()
            {
                var reader = new StreamReader(Stream, Encoding.UTF8);

                while (Remote.Connected)
                {
                    if (Remote.Client.Available > 0)
                    {
                        var builder = new StringBuilder();
                        var buffer = new byte[BufferSize];
                        int read;

                        do
                        {
                            read = Stream.Read(buffer, 0, buffer.Length);

                            builder.Append(Encoding.UTF8.GetString(buffer, 0, read).TrimEnd('\0'));
                        }
                        while (Remote.Client.Available > 0);

                        DataReceived(builder.ToString());
                        Send(builder.ToString());
                    }
                }
            }
        }
    }
}
