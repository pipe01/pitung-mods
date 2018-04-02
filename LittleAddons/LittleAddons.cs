using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThroughInverter;

namespace LittleAddons
{
    public class LittleAddons : Mod
    {
        public override string Name => "LittleAddons";
        public override string PackageName => "me.pipe01.LittleAddons";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        public override void BeforePatch()
        {
            ThroughInverter.ThroughInverter.Register(this);
        }
    }
}
