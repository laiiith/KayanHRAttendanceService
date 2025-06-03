using KayanHRAttendanceService.Application.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KayanHRAttendanceService.Application.Implementation.Services;

public class HttpService(IHttpClientFactory httpClientFactory) : IHttpService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<ApiResponse<TResponse>> SendAsync<TResponse>(APIRequest apiRequest)
    {
        using var requestMessage = BuildHttpRequestMessage(apiRequest);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(apiRequest.TimeoutSeconds ?? 30));

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(requestMessage, cts.Token);

            if (apiRequest.IncludeHeaders)
            {
                var headers = response.Headers.Concat(response.Content.Headers)
                    .ToDictionary(h => h.Key, h => string.Join(",", h.Value));
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (!response.IsSuccessStatusCode)
                return ApiResponse<TResponse>.Fail($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {content}", (int)response.StatusCode);

            TResponse? deserialized = DeserializeResponse<TResponse>(content, apiRequest.ResponseContentType);
            return ApiResponse<TResponse>.Success(deserialized!, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<TResponse>.Fail($"Exception: {ex.Message}", 500);
        }
    }

    private HttpRequestMessage BuildHttpRequestMessage(APIRequest apiRequest)
    {
        var url = AppendQueryParameters(apiRequest.Url, apiRequest.QueryParameters);

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(url),
            Method = apiRequest.Method
        };

        AddRequestContent(apiRequest, request);
        AddRequestHeaders(apiRequest, request);

        return request;
    }

    private string AppendQueryParameters(string baseUrl, Dictionary<string, string>? queryParams)
    {
        if (queryParams is null || !queryParams.Any())
            return baseUrl;

        var query = string.Join("&", queryParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        var separator = baseUrl.Contains('?') ? "&" : "?";
        return $"{baseUrl}{separator}{query}";
    }

    private void AddRequestContent(APIRequest apiRequest, HttpRequestMessage request)
    {
        if (apiRequest.Data is null)
            return;

        request.Content = apiRequest.RequestContentType switch
        {
            HttpServiceContentTypes.x_www_FormURLurlencoded => new FormUrlEncodedContent(apiRequest.Data.GetType().GetProperties().Select(p => new KeyValuePair<string, string>(p.Name, p.GetValue(apiRequest.Data)?.ToString() ?? ""))),
            HttpServiceContentTypes.application_json => new StringContent(JsonSerializer.Serialize(apiRequest.Data), Encoding.UTF8, "application/json"),
            HttpServiceContentTypes.text_plain => new StringContent(apiRequest.Data.ToString() ?? string.Empty, Encoding.UTF8, "text/plain"),
            _ => throw new NotSupportedException($"Content type {apiRequest.RequestContentType} is not supported.")
        };
    }

    private void AddRequestHeaders(APIRequest apiRequest, HttpRequestMessage request)
    {
        var contentType = GetMediaType(apiRequest.RequestContentType);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

        if (!string.IsNullOrWhiteSpace(apiRequest.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.Token);
        }

        if (apiRequest.CustomHeaders != null)
        {
            foreach (var header in apiRequest.CustomHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private string GetMediaType(HttpServiceContentTypes type) => type switch
    {
        HttpServiceContentTypes.x_www_FormURLurlencoded => "application/x-www-form-urlencoded",
        HttpServiceContentTypes.application_json => "application/json",
        HttpServiceContentTypes.text_plain => "text/plain",
        _ => "application/octet-stream"
    };

    private TResponse DeserializeResponse<TResponse>(string content, HttpServiceContentTypes? responseContentType)
    {
        if (string.IsNullOrWhiteSpace(content))
            return default!;

        if (responseContentType == HttpServiceContentTypes.text_plain && typeof(TResponse) == typeof(string))
        {
            return (TResponse)(object)content;
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new JsonException("Deserialization returned null.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to deserialize the response content.", ex);
        }
    }
}