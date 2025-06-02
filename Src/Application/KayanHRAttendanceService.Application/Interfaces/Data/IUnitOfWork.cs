namespace KayanHRAttendanceService.Application.Interfaces.Data;

public interface IUnitOfWork
{
    IAttendanceDataRepository AttendanceData { get; }

    Task Save();
}