using System.Text.Json.Serialization;

namespace KayanHRAttendanceService.Application.DTO;
public record TokenDTO
{
    [JsonPropertyName("token")]
    public string AccessToken { get; set; }
}