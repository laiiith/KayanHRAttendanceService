using Newtonsoft.Json;

namespace KayanHRAttendanceService.Application.DTO;
public record TokenDTO
{
    [JsonProperty("token")]
    public string AccessToken { get; set; }
}