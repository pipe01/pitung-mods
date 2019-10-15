using PiTung.Components;
using PiTung.Mod_utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LittleAddons
{
    internal class GateXOR : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Back, SideType.Input)
                .WithIO(CubeSide.Top, SideType.Input)
                .WithIO(CubeSide.Front, SideType.Output);

            if (Configuration.Get("ColoredGates", true))
                b = b.WithColor(new Color(0.2f, 0.7f, 0.2f));

            ComponentRegistry.CreateNew<GateXOR>("xorgate", "XOR Gate", b);
        }

        protected override void CircuitLogicUpdate()
        {
            this.Outputs[0].On = this.Inputs[0].On != this.Inputs[1].On;
        }
    }
}
