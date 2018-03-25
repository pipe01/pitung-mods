using System.Net;
using NetSockets;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoMod_Server
{
    class Program
    {
        static void Main(string[] args) => new Program().Main();

        NetObjectServer objServer = new NetObjectServer();
        NetStringServer server = new NetStringServer();

        public void Main()
        {
            server.Start(IPAddress.Loopback, 5757);

            server.OnClientConnected += Server_OnClientConnected;
            server.OnClientRejected += Server_OnClientRejected;
            server.OnReceived += Server_OnReceived;
            
            new ManualResetEventSlim().Wait();
        }

        private void Server_OnClientRejected(object sender, NetClientRejectedEventArgs e)
        {
            Console.WriteLine("Rejected");
        }

        private void Server_OnReceived(object sender, NetClientReceivedEventArgs<string> e)
        {
            Console.WriteLine("< " + e.Data);
        }

        private void Server_OnClientConnected(object sender, NetClientConnectedEventArgs e)
        {
            e.Reject = false;
            Console.WriteLine("New client: " + e.Guid);

            server.DispatchTo(e.Guid, "hola que tal");
        }
    }
}
