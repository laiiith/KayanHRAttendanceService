using Newtonsoft.Json;

namespace KayanHRAttendanceService.Application.AttendanceConnector.DTO;

public record TokenDTO
{
    [JsonProperty("token")]
    public string AccessToken { get; set; }
}
