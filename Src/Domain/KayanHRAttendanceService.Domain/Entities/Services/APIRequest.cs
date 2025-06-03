namespace KayanHRAttendanceService.Domain.Entities.Services;

public class APIRequest
{
    public string Url { get; set; } = string.Empty;
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public object? Data { get; set; }
    public string? Token { get; set; }
    public Dictionary<string, string>? CustomHeaders { get; set; }
    public Dictionary<string, string>? QueryParameters { get; set; }
    public HttpServiceContentTypes RequestContentType { get; set; } = HttpServiceContentTypes.application_json;
    public HttpServiceContentTypes? ResponseContentType { get; set; } = HttpServiceContentTypes.application_json;
    public int? TimeoutSeconds { get; set; }
    public bool IncludeHeaders { get; set; }
}

public enum HttpServiceContentTypes
{
    application_json, x_www_FormURLurlencoded, text_plain
}