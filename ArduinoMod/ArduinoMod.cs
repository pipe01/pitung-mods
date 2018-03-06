using PiTung.Console;
using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArduinoMod
{
    public class ArduinoMod : Mod
    {
        public override string Name => "Arduino";
        public override string PackageName => "me.pipe01.ArduinoMod";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        public override void BeforePatch()
        {

        }
    }
}
