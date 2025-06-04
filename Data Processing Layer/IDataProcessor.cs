using GateWay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Data_Processing_Layer
{
    public interface IDataProcessor
    {
        Task ProcessDataAsync(List<RealTimeData> data, CancellationToken token);
    }
}
