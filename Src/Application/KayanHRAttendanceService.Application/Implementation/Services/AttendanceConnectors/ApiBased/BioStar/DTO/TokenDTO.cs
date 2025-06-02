using Newtonsoft.Json;

namespace KayanHRAttendanceService.Infrastructure.Services.AttendanceConnectors.ApiBased.BioStar.DTO;
public record TokenDTO
{
    [JsonProperty("token")]
    public string AccessToken { get; set; }
}
