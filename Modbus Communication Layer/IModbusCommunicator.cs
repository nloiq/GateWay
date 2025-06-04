using GateWay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Modbus_Communication_Layer
{
    public interface IModbusCommunicator : IDisposable
    {
        Task<List<RealTimeData>> ReadRegistersAsync(List<ModbusRegister> registers, byte unitId, CancellationToken token);
    }
}
