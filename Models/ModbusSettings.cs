using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Models
{
    public class ModbusSettings
    {
        public string RegisterFile { get; set; }
        public byte UnitId { get; set; }
        public int PollingIntervalSeconds { get; set; } = 10;

        // RS485 settings
        public string SerialPortName { get; set; }
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public string Parity { get; set; } = "None";
        public string StopBits { get; set; } = "One";
        public int ReadTimeout { get; set; } = 1000;
        public int WriteTimeout { get; set; } = 1000;

        // TCP settings
        public string Host { get; set; }
        public int Port { get; set; } = 502;

        // Common settings
        public int Retries { get; set; } = 3;
        public int WaitToRetryMilliseconds { get; set; } = 1000;
    }
}
