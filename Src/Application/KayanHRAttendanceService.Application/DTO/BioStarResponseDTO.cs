using Newtonsoft.Json;

namespace KayanHRAttendanceService.Application.DTO;
public record BioStarResponseDTO
{
    [JsonProperty("index")]
    public string ID { get; set; }
    [JsonProperty("user_id")]
    public string EmployeeCode { get; set; }
    [JsonProperty("datetime")]
    public string PunchTime { get; set; }
    [JsonProperty("device_id")]
    public string MachineName { get; set; }
    [JsonProperty("terminal_sn")]
    public string MachineSerialNo { get; set; }
    [JsonProperty("function")]
    public string PunchStatus { get; set; }
}