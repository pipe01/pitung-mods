using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestMod
{
    public static class Radio
    {
        public static IList<Receiver> Receivers = new List<Receiver>();
        public static IDictionary<int, bool> Channels = new Dictionary<int, bool>();
    }
}
