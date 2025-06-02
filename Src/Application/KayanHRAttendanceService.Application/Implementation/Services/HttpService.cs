using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KayanHRAttendanceService.Application.Implementation.Services
{
    public class HttpService(IHttpClientFactory httpClientFactory) : IHttpService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

        public async Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true)
        {
            using var requestMessage = new HttpRequestMessage(apiRequest.Method, apiRequest.Url);

            if (apiRequest.Data != null)
            {
                var json = JsonSerializer.Serialize(apiRequest.Data);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            foreach (var header in apiRequest.CustomHeaders)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (withBearer)
            {
                var token = GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private string GetToken()
        {
            return "your-access-token";
        }
    }
}