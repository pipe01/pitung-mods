using PiTung;
using PiTung.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LittleAddons
{
    internal class DLatch : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Front, SideType.Output)
                .WithIO(CubeSide.Top, SideType.Input, "Clock")
                .WithIO(CubeSide.Back, SideType.Input, "Data")
                .WithColor(new Color(0, 0.6f, 0));

            ComponentRegistry.CreateNew<DLatch>("dlatch", "D Latch", b);
        }

        protected override void CircuitLogicUpdate()
        {
            if (Inputs[0].On)
                Outputs[0].On = Inputs[1].On;
        }
    }
}
