using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Models
{
    public class ModbusRegister
    {
        public int Address { get; set; }
        public string Type { get; set; } // "SPI", "DPI", "AMI"
        public string Description { get; set; }
    }

}
