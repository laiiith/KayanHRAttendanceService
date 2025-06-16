public class KeyTechApiResponseDTO
{
    public string Result { get; set; } = string.Empty;
    public List<KeyTechPunchDTO>? Data { get; set; }
}

public class KeyTechPunchDTO
{
    public int Index { get; set; }
    public string EmployeeCode { get; set; }
    public string Timestamp { get; set; }
    public string Type { get; set; }
    public string Machine { get; set; }
}
