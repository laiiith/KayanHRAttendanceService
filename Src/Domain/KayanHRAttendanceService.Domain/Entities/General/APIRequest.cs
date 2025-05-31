namespace KayanHRAttendanceService.Domain.Entities.General;

public class APIRequest
{
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string Url { get; set; }
    public object Data { get; set; }
    public HttpServiceContentTypes RequestContentType { get; set; } = HttpServiceContentTypes.application_json;
    public string Token { get; set; }
    public Dictionary<string, string> CustomHeaders { get; set; }
    public Dictionary<string, string> QueryParameters { get; set; }
}
public enum HttpServiceContentTypes
{
    application_json, x_www_FormURLurlencoded, text_plain
}