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
            var client = new Client();

            Console.WriteLine("Connecting...");

            client.Start();
            client.DataReceived += (_, d) => Console.Write("< " + d);

            new ManualResetEventSlim().Wait();
        }
    }
}
