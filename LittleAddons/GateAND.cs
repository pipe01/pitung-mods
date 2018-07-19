using PiTung.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LittleAddons
{
    internal class GateAND : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Front, SideType.Output)
                .WithIO(CubeSide.Back, SideType.Input, -0.25f, 0)
                .WithIO(CubeSide.Back, SideType.Input, 0.25f, 0)
                .WithColor(new Color(0, 0.2f, 0.7f));
           
            ComponentRegistry.CreateNew<GateAND>("andgate", "AND Gate", b);
        }

        protected override void CircuitLogicUpdate()
        {
            this.Outputs[0].On = this.Inputs[0].On && this.Inputs[1].On;
        }
    }
}
