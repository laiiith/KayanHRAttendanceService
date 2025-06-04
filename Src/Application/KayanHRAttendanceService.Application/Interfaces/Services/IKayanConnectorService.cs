using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Application.Interfaces.Services;

public interface IKayanConnectorService
{
    Task PushToKayanConnectorEndPoint(List<AttendanceRecord>? records);
}