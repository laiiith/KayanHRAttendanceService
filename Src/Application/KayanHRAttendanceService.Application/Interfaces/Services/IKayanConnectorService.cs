using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Application.Interfaces.Services;

public interface IKayanConnectorService
{
    Task<(bool IsSuccess, int StatusID)> PushToKayanConnectorEndPoint(List<AttendanceRecord> records);
}