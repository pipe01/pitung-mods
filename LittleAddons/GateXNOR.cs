using PiTung.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LittleAddons
{
    internal class GateXNOR : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Back, SideType.Input)
                .WithIO(CubeSide.Top, SideType.Input)
                .WithIO(CubeSide.Front, SideType.Output)
                .WithColor(new Color(0.2f, 0.2f, 0.5f));

            ComponentRegistry.CreateNew<GateXNOR>("xnorgate", "XNOR Gate", b);
        }

        protected override void CircuitLogicUpdate()
        {
            this.Outputs[0].On = this.Inputs[0].On == this.Inputs[1].On;
        }
    }
}
