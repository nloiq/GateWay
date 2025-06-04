using GateWay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Data_Logging_Layer
{
    public interface IDataLogger
    {
        Task AppendAsync(List<RealTimeData> data, CancellationToken token);
    }
}
