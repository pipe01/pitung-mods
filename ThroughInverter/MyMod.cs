using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThroughInverter
{
    public class MyMod : Mod
    {
        public override string Name => "Through Inverters";
        public override string PackageName => "me.pipe01.ThroughInverters";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public static MyMod Instance { get; private set; }

        public MyMod()
        {
            Instance = this;
        }

        public override void BeforePatch()
        {
            MyInverter.Register();
        }
    }
}
