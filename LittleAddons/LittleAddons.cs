using PiTung;
using References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LittleAddons
{
    public class LittleAddons : Mod
    {
        public override string Name => "LittleAddons";
        public override string PackageName => "me.pipe01.LittleAddons";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.5");
        public override string UpdateUrl => "http://pipe0481.heliohost.org/pitung/mods/manifest.ptm";

        public override void BeforePatch()
        {
            ThroughInverter.Register();
            KeyButton.Register();
            TFlipFlop.Register();
            DLatch.Register();
            FileReader.Register();
            EdgeDetector.Register();
            GateAND.Register();
            GateNAND.Register();
            GateOR.Register();
        }
        
        internal static void PlayButtonSound()
        {
            SoundPlayer.PlaySoundGlobal(Sounds.UIButton);
        }
    }
}
