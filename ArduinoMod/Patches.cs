using Harmony;
using PiTung.Console;
using PiTung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArduinoMod
{
    [Target(typeof(CircuitInput))]
    public static class CircuitInputPatch
    {
        [PatchMethod]
        public static void CircuitLogicUpdate(CircuitInput __instance)
        {
            var lamp = ArduinoMod.ActiveLamps.SingleOrDefault(o => o.LampObject == __instance.transform.parent.gameObject);

            if (lamp == null)
                return;

            bool newState = false;
            if (__instance.Cluster != null)
            {
                newState = __instance.Cluster.On;
            }

            if (newState != __instance.On)
            {
                lamp.Set(newState);
            }
        }
    }
}
