using GateWay.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace GateWay.API_Communication_Layer
{
    public class ApiClient : IApiClient
    {
        private readonly ILogger<ApiClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public ApiClient(
            ILogger<ApiClient> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<ApiSettings> apiOptions)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(apiOptions.Value.TimeoutSeconds);
            _apiSettings = apiOptions.Value;
        }

        public async Task PostDataAsync(List<RealTimeData> data, CancellationToken token)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiSettings.BaseUrl, data, token);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Data successfully posted to API");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync(token);
                    _logger.LogWarning("API returned {StatusCode}: {Response}",
                        response.StatusCode,
                        responseContent.Length > 200 ? responseContent[..200] + "..." : responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to post data to API");
            }
        }
    }
}
