using NModbus.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace GateWay.Helper_Classes
{
    public static class ModbusTransportExtensions
    {
        public static bool IsSocketConnected(this ModbusIpTransport transport)
        {
            try
            {
                var socket = transport.GetType()
                    .GetProperty("Socket", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
                    .GetValue(transport) as Socket;

                return socket?.Connected == true &&
                       !(socket.Poll(1000, SelectMode.SelectRead) &&
                       socket.Available == 0);
            }
            catch
            {
                return false;
            }
        }
    }
}
