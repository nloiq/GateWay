using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Models
{
    public class ModBusProtocolSettings
    {
        public string Type { get; set; } // "RS485" or "TCP"
        public bool EnableAutoReconnect { get; set; } = true;
        public int MaxReconnectAttempts { get; set; } = 5;
    }
}
