using System.Collections.Generic;
using System.Text;
using OpenNETCF.IO.Ports;

namespace ArduinoMod
{
    internal class Arduino
    {
        private class Packet
        {
            public byte Code, Value;

            public Packet()
            {
            }
            public Packet(byte code, byte value)
            {
                this.Code = code;
                this.Value = value;
            }
        }

        public static Arduino Instance { get; } = new Arduino();

        public delegate void PinChangedDelegate(int pin, bool value);
        public event PinChangedDelegate PinChanged = delegate { };

        public SerialPort Serial;

        private Queue<byte> LastBytes = new Queue<byte>();

        private Arduino()
        {
        }

        public void Init()
        {
            Serial = new SerialPort("COM3", 57200);
            Serial.ReceivedEvent += Serial_ReceivedEvent;
            Serial.Open();
        }

        private void Serial_ReceivedEvent(object sender, SerialReceivedEventArgs e)
        {
            int b;

            while ((b = Serial.ReadByte()) != -1)
            {
                LastBytes.Enqueue((byte)b);
            }

            while (ParsePacket()) ;

            bool ParsePacket()
            {
                if (LastBytes.Count < 3)
                    return false;

                var bs = new byte[3];

                for (int i = 0; i < 3; i++)
                {
                    bs[i] = LastBytes.Dequeue();
                }

                if (bs[3] == 255)
                {
                    OnPacketReceived(new Packet(bs[0], bs[1]));
                    return true;
                }

                return false;
            }
        }

        private void OnPacketReceived(Packet packet)
        {
            PinChanged(packet.Code, packet.Value == 1);
        }

        public void Send(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            Serial.Write(bytes, 0, bytes.Length);
        }
    }
}
