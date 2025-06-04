using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Helper_Classes
{
    public static class SerialPortHelper
    {
        public static Parity GetParity(string parity) => parity switch
        {
            "None" => Parity.None,
            "Odd" => Parity.Odd,
            "Even" => Parity.Even,
            "Mark" => Parity.Mark,
            "Space" => Parity.Space,
            _ => throw new ArgumentException($"Invalid parity value: {parity}")
        };

        public static StopBits GetStopBits(string stopBits) => stopBits switch
        {
            "None" => StopBits.None,
            "One" => StopBits.One,
            "Two" => StopBits.Two,
            "OnePointFive" => StopBits.OnePointFive,
            _ => throw new ArgumentException($"Invalid stop bits value: {stopBits}")
        };
    }
}
