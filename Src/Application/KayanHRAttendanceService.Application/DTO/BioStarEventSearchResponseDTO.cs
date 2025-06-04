using System.Text.Json.Serialization;

namespace KayanHRAttendanceService.Application.DTO;
public record BioStarEventSearchResponseDTO
{
    [JsonPropertyName("EventCollection")]
    public EventCollection EventCollection { get; init; }
}
public record EventCollection
{
    [JsonPropertyName("rows")]
    public List<BioStarEventDTO> Rows { get; init; } = [];
}

public record BioStarEventDTO
{
    public int Index { get; init; }

    public UserIdWrapper? User_Id { get; init; }

    public string? Datetime { get; init; }

    public EventTypeIdWrapper? Event_Type_Id { get; init; }

    public string? Tna_Key { get; init; }

    public DeviceIdWrapper? Device_Id { get; init; }
}

public record UserIdWrapper
{
    public string? User_Id { get; init; }
}

public record EventTypeIdWrapper
{
    public string? Code { get; init; }
}

public record DeviceIdWrapper
{
    public string? Name { get; init; }
}