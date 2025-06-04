using KayanHRAttendanceService.Application.DTO;
using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Application.Interfaces.Services;

public interface IKayanConnectorService
{
    Task<(KayanConnectorResponseDTO? response, int StatusID)> PushToKayanConnectorEndPoint(List<AttendanceRecord> records);
}