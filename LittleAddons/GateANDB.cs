using PiTung.Components;
using PiTung.Mod_utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LittleAddons
{
    internal class GateANDB : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Front, SideType.Output)
                .WithIO(CubeSide.Top, SideType.Input, -0.25f, 0)
                .WithIO(CubeSide.Top, SideType.Input, 0.25f, 0);

            if (Configuration.Get("ColoredGates", true))
                b = b.WithColor(new Color(0, 0.2f, 0.7f));

            ComponentRegistry.CreateNew<GateANDB>("andgateb", "AND Gate alt", b);
        }

        protected override void CircuitLogicUpdate()
        {
            this.Outputs[0].On = this.Inputs[0].On && this.Inputs[1].On;
        }
    }
}
