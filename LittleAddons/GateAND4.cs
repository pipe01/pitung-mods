using PiTung.Components;
using PiTung.Mod_utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LittleAddons
{
    internal class GateAND4 : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Front, SideType.Output)
                .WithIO(CubeSide.Back, SideType.Input, 0.25f, 0.25f)
                .WithIO(CubeSide.Back, SideType.Input, 0.25f, -0.25f)
                .WithIO(CubeSide.Back, SideType.Input, -0.25f, 0.25f)
                .WithIO(CubeSide.Back, SideType.Input, -0.25f, -0.25f);

            if (Configuration.Get("ColoredGates", true))
                b = b.WithColor(new Color(0, 0.2f, 0.7f));

            ComponentRegistry.CreateNew<GateAND4>("andgate4", "AND Gate (4 in)", b);
        }

        protected override void CircuitLogicUpdate()
        {
            this.Outputs[0].On = this.Inputs.All(o => o.On);
        }
    }
}
