using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArduinoMod
{
    class ArduLamp
    {
        public GameObject LampObject { get; set; }
        public int ArduinoPin { get; set; }

        public void Set(bool value)
        {
            Arduino.Instance.Serial.Write(new byte[] { (byte)(value ? 2 : 1), (byte)this.ArduinoPin }, 0, 2);
        }
    }
}
