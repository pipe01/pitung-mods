using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleAddons
{
    public class LittleAddons : Mod
    {
        public override string Name => "LittleAddons";
        public override string PackageName => "me.pipe01.LittleAddons";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public override void BeforePatch()
        {
            ThroughInverter.Register(this);
            KeyButton.Register(this);
        }
    }
}
