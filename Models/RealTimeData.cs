using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Models
{
    public class RealTimeData
    {
        public int Address { get; set; }
        public int Value { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
