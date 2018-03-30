using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestMod;

namespace WirelessMod
{
    public class WirelessMod : Mod
    {
        public override string Name => "WirelessMod";
        public override string PackageName => "me.pipe01.WirelessMod";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        public override void BeforePatch()
        {
            Receiver.Register();
            Transmitter.Register();
        }
    }
}
