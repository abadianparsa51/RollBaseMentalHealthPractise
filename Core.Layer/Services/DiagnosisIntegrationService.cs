using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Core.Model.Layer.Dto;
using Microsoft.Extensions.Logging;

namespace Core.Layer.Services
{
    public class DiagnosisIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DiagnosisIntegrationService> _logger;

        public DiagnosisIntegrationService(IHttpClientFactory httpClientFactory, ILogger<DiagnosisIntegrationService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("PythonAPI");

            // اضافه کردن Authorization header
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "neuroease-python-token");

            _logger = logger;
        }

        public async Task<DiagnosisResultDto?> GetDiagnosisAsync(DiagnosisRequestDto request)
        {
            HttpResponseMessage? response = null;
            try
            {
                _logger.LogInformation("Sending request to Python API: {@Request}", request);

                response = await _httpClient.PostAsJsonAsync("/diagnose", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<DiagnosisResultDto>();
                _logger.LogInformation("Received response from Python API: {@Result}", result);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Python API");

                if (response != null)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Python API returned error content: {ErrorContent}", errorContent);
                }

                return null;
            }
        }
    }
}
