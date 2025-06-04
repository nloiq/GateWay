using GateWay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.API_Communication_Layer
{
    public interface IApiClient
    {
        Task PostDataAsync(List<RealTimeData> data, CancellationToken token);
    }
}
