namespace KayanHRAttendanceService.Domain.Interfaces;
public interface IUnitOfWork
{
    IAttendanceDataRepository AttendanceData { get; }
    Task Save();
}
