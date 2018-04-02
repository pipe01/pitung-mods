using PiTung.Components;
using UnityEngine;

namespace TestMod
{
    public class Receiver : NetworkComponent
    {
        public static void Register()
        {
            var prefab = PrefabBuilder
                .Cube
                .WithSide(CubeSide.Top, SideType.Output)
                .WithColor(new Color(0.7f, 0.7f, 1))
                .WithComponent<InteractNetwork>();

            ComponentRegistry.CreateNew<Receiver>(WirelessMod.WirelessMod.Instance, "receiver", "Receiver", prefab);
        }

        void Start()
        {
            GetComponent<InteractNetwork>().Component = this;
            Radio.Receivers.Add(this);
        }
        
        public override void UpdateFrequency()
        {
            this.QueueCircuitLogicUpdate();
        }

        protected override void CircuitLogicUpdate()
        {
            if (!Radio.Channels.TryGetValue(Frequency, out var s))
                Radio.Channels[Frequency] = false;

            Outputs[0].On = s;
        }
    }
}
