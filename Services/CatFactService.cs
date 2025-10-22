using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ProfileApi.Services
{
    public class CatFactService
    {
        private readonly HttpClient _httpClient;

        public CatFactService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(5); // Set timeout for API call
        }

        public async Task<(bool Success, string Fact)> GetRandomFactAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<CatFactResponse>("https://catfact.ninja/fact");
                if (!string.IsNullOrEmpty(response?.fact))
                    return (true, response.fact);

                return (false, "Could not fetch a cat fact at this time.");
            }
            catch
            {
                return (false, "Could not fetch a cat fact at this time.");
            }
        }

        private class CatFactResponse
        {
            public string fact { get; set; }
        }
    }
}
