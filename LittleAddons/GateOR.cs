using PiTung.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LittleAddons
{
    internal class GateOR : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Back, SideType.Input)
                .WithIO(CubeSide.Top, SideType.Input)
                .WithIO(CubeSide.Front, SideType.Output)
                .WithColor(new Color(0.5f, 0, 0));

            ComponentRegistry.CreateNew<GateOR>("orgate", "OR Gate", b);
        }

        protected override void CircuitLogicUpdate()
        {
            this.Outputs[0].On = this.Inputs[0].On || this.Inputs[1].On;
        }
    }
}
