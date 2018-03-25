using System.Linq;
using PiTung.Mod_utilities;
using System.Text;
using PiTung.Console;
using PiTung;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArduinoMod
{
    public class ArduinoMod : Mod
    {
        public override string Name => "Arduino";
        public override string PackageName => "me.pipe01.ArduinoMod";
        public override string Author => "pipe01";
        public override Version ModVersion => new Version("1.0.0");

        internal static List<ArduLamp> ActiveLamps = new List<ArduLamp>();

        public override void BeforePatch()
        {
            Arduino.Instance.Init();
            
            ModInput.RegisterBinding(this, "LampAction", KeyCode.H).ListenKeyDown(LampAction);
            ModInput.RegisterBinding(this, "LampPinAdd", KeyCode.UpArrow).ListenKeyDown(PinDownKey);
        }

        private void PinDownKey()
        {
            throw new NotImplementedException();
        }

        public override void OnApplicationQuit()
        {
            Arduino.Instance.Serial.Close();
        }

        private void LampAction()
        {
            var lamp = GetLamp();

            if (lamp != null && ActiveLamps.RemoveAll(o => o.LampObject == lamp) == 0)
            {
                ActiveLamps.Add(new ArduLamp
                {
                    ArduinoPin = 13,
                    LampObject = lamp
                });
            }
        }

        private GameObject GetLamp()
        {
            Transform transform = FirstPersonInteraction.FirstPersonCamera.transform;

            if (Physics.Raycast(transform.position, transform.forward, out var hit, MiscellaneousSettings.ReachDistance)
                && hit.transform.parent.gameObject.name.StartsWith("Display"))
            {
                return hit.transform.parent.gameObject;
            }

            return null;
        }
    }
}
