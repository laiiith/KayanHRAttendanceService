﻿using System.ComponentModel.DataAnnotations;

namespace KayanHRAttendanceService.Domain.Entities.Sqlite;

public record AttendanceRecord
{
    [Key]
    public int ID { get; set; }
    public string? EmployeeCode { get; set; }
    public required string PunchTime { get; set; }
    public string? Function { get; set; }
    public string? MachineName { get; set; }
    public string? MachineSerialNo { get; set; }
    public string? Status { get; set; }
    public required string TId { get; set; }
}