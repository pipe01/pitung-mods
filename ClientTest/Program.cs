using NetSockets;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);

            Console.WriteLine("Connecting...");

            var mre = new ManualResetEventSlim();

            var client = new NetStringClient();

            while (!client.TryConnect("127.0.0.1", 5757)) ;
            
            client.OnConnected += Client_OnConnected;
            client.OnReceived += Client_OnReceived;
            client.OnDisconnected += (a, b) =>
            {
                Console.WriteLine("Disconnected");
                mre.Set();
            };

            mre.Wait();
        }

        private static void Client_OnReceived(object sender, NetReceivedEventArgs<string> e)
        {
            Console.WriteLine("------> " + e.Data);
        }

        private static void Client_OnConnected(object sender, NetConnectedEventArgs e)
        {
            Console.WriteLine("------- Connected!");
        }
    }
}
