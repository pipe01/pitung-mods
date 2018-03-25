using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArduinoMod
{
    public class Command_serial : Command
    {
        public override string Name => "serial";
        public override string Usage => $"{Name} [reconnect]";
        public override string Description => "ArduinoMod's main command";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (!arguments.Any())
                return false;



            return true;
        }
    }
}
