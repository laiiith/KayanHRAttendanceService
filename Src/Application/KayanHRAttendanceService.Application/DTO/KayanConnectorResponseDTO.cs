using Newtonsoft.Json;

namespace KayanHRAttendanceService.Application.DTO;

public class KayanConnectorResponseDTO
{
    public Response Response { get; set; }
    public Data Data { get; set; }
}
public class Response
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}
public class Data
{
    [JsonProperty("lstpunches")]
    public List<KayanConnectorAttendanceDTO> ListPunches { get; set; }
}