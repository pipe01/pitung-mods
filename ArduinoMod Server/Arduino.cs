using System.IO.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoMod_Server
{
    public class Arduino
    {
        private SerialPort Serial;

        public void Init(string port, int baudRate)
        {
            Serial = new SerialPort(port, baudRate);
            Serial.Open();

            Serial.DataReceived += Serial_DataReceived;
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
