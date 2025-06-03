using Newtonsoft.Json;

namespace KayanHRAttendanceService.Application.DTO;
public record BioTimeResponseDTO
{
    [JsonProperty("data")]
    public List<BioTimePunches> BioTimePunches { get; set; }
}

public record BioTimePunches
{
    [JsonProperty("id")]
    public string ID { get; set; }
    [JsonProperty("emp_code")]
    public string EmployeeCode { get; set; }
    [JsonProperty("punch_time")]
    public string PunchTime { get; set; }
    [JsonProperty("terminal_alias")]
    public string MachineName { get; set; }
    [JsonProperty("terminal_sn")]
    public string MachineSerialNo { get; set; }
    [JsonProperty("punch_state")]
    public string PunchStatus { get; set; }
}