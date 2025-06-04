using GateWay.API_Communication_Layer;
using GateWay.Data_Logging_Layer;
using GateWay.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.Data_Processing_Layer
{
    public class DataProcessor : IDataProcessor
    {
        private readonly ILogger<DataProcessor> _logger;
        private readonly IApiClient _apiClient;
        private readonly IDataLogger _dataLogger;
        private readonly ApiSettings _apiSettings;

        public DataProcessor(
            ILogger<DataProcessor> logger,
            IApiClient apiClient,
            IDataLogger dataLogger,
            IOptions<ApiSettings> apiOptions)
        {
            _logger = logger;
            _apiClient = apiClient;
            _dataLogger = dataLogger;
            _apiSettings = apiOptions.Value;
        }

        public async Task ProcessDataAsync(List<RealTimeData> data, CancellationToken token)
        {
            try
            {
                // LOCAL STORAGE
                await _dataLogger.AppendAsync(data, token);

                // API COMMUNICATION (if configured)
                if (!string.IsNullOrEmpty(_apiSettings.BaseUrl))
                {
                    await _apiClient.PostDataAsync(data, token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data");
                throw;
            }
        }
    }
}
