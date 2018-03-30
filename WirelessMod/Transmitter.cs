using PiTung.Components;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TestMod
{
    public class Transmitter : NetworkComponent
    {
        public static void Register()
        {
            var prefab = PrefabBuilder
                .Cube
                .WithSide(CubeSide.Top, SideType.Input)
                .WithColor(new Color(1, 0.7f, 0.7f))
                .WithComponent<InteractNetwork>();
            
            ComponentRegistry.CreateNew<Transmitter>("transmitter", "Transmitter", prefab);
        }
        
        void Start()
        {
            GetComponent<InteractNetwork>().Component = this;
            IGConsole.Log(this.gameObject.name);
        }
        
        protected override void CircuitLogicUpdate()
        {
            Radio.Channels[Frequency] = Inputs[0].On;
                
            foreach (var item in Radio.Receivers.Where(o => o.Frequency == this.Frequency))
            {
                item?.UpdateFrequency();
            }
        }
    }
}
