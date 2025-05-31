namespace KayanHRAttendanceService.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IAttendanceDataRepository AttendanceData { get; }
    Task Save();
}
