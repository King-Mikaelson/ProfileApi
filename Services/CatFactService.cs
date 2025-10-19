using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ProfileApi.Services
{
    public class CatFactService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatFactService> _logger;

        // fallback message used when external API fails
        private const string FALLBACK = "Could not fetch cat fact at this time.";

        public CatFactService(HttpClient httpClient, ILogger<CatFactService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Defensive: ensure a reasonable timeout even if not set elsewhere
            if (_httpClient.Timeout == System.Threading.Timeout.InfiniteTimeSpan)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(5);
            }
        }

        /// <summary>
        /// Returns a random cat fact or a safe fallback if the external API fails.
        /// Does not throw; logs errors and returns fallback string.
        /// </summary>
        public async Task<string> GetRandomFactAsync()
        {
            try
            {
                using var response = await _httpClient.GetAsync("https://catfact.ninja/fact");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CatFacts API returned non-success status {StatusCode}", response.StatusCode);
                    return FALLBACK;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                if (doc.RootElement.TryGetProperty("fact", out var factProp))
                {
                    return factProp.GetString() ?? FALLBACK;
                }

                _logger.LogWarning("CatFacts API response did not contain 'fact' property.");
                return FALLBACK;
            }
            catch (TaskCanceledException tce)
            {
                // Timeout or explicit cancellation
                _logger.LogError(tce, "CatFacts API request timed out.");
                return FALLBACK;
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Network error while calling CatFacts API.");
                return FALLBACK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CatFactService.");
                return FALLBACK;
            }
        }
    }
}
