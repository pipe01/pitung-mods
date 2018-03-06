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

        private Server Server;
        private ManualResetEventSlim ExitEvent = new ManualResetEventSlim();

        public void Main()
        {
            Console.WriteLine("Starting server...");

            Server = new Server();
            Server.DebugLog += (_, e) => Console.WriteLine("-> " + e);
            Server.Start();

            Console.WriteLine("Listening");

            ExitEvent.Wait();

            Server.Stop(false);
        }
    }
}
