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

        public event EventHandler<string> Log = delegate { };
        public event EventHandler<string> DebugLog = delegate { };

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

                DebugLog(this, "Connected to client.");

                Send("hi");
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
        
        private void Send(string str)
        {
            if (Remote == null)
                return;

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            var stream = Remote.GetStream();

            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
    }
}
