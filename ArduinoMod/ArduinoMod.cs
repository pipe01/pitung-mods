using ClientTest;
using PiTung.Console;
using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArduinoMod
{
    public class ArduinoMod : Mod
    {
        public override string Name => "Arduino";
        public override string PackageName => "me.pipe01.ArduinoMod";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        private Client Client;

        public override void BeforePatch()
        {
            Client = new Client();
            Client.Start();
            Client.DataReceived += d => Console.WriteLine("---------------> " + d);
        }

        public override void OnApplicationQuit()
        {
            Client?.Stop();
        }
    }
}
