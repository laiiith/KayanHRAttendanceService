using System.Text.Json.Serialization;

namespace KayanHRAttendanceService.Application.DTO;
public record BioTimeResponseDTO
{
    [JsonPropertyName("data")]
    public List<BioTimePunches> BioTimePunches { get; set; }
    [JsonPropertyName("next")]
    public string NextUrl { get; set; }
}

public record BioTimePunches
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("emp_code")]
    public string EmployeeCode { get; set; }
    [JsonPropertyName("punch_time")]
    public string PunchTime { get; set; }
    [JsonPropertyName("terminal_alias")]
    public string MachineName { get; set; }
    [JsonPropertyName("terminal_sn")]
    public string MachineSerialNo { get; set; }
    [JsonPropertyName("punch_state")]
    public string PunchStatus { get; set; }
}