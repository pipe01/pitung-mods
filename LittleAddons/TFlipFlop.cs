using PiTung;
using PiTung.Components;
using UnityEngine;

namespace LittleAddons
{
    internal class TFlipFlop : UpdateHandler
    {
        public static void Register()
        {
            var b = PrefabBuilder
                .Cube
                .WithIO(CubeSide.Top, SideType.Input)
                .WithIO(CubeSide.Front, SideType.Output)
                .WithColor(new Color(0.409803922f, 0.647058824f, 0.968627451f));

            ComponentRegistry.CreateNew<TFlipFlop>("tflipflop", "T Flip-Flop", b);
        }

        protected override void CircuitLogicUpdate()
        {
            if (Inputs[0].On)
                Outputs[0].On = !Outputs[0].On;
        }
    }
}
