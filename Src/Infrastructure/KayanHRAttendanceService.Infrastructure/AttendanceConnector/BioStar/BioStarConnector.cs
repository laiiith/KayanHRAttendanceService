using KayanHRAttendanceService.Application.AttendanceConnector.Interfaces;
using KayanHRAttendanceService.Domain.Entities.Sqlite;

namespace KayanHRAttendanceService.Infrastructure.AttendanceConnector.BioStar;

public class BioStarConnector : AttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceDataAsync()
    {
        throw new NotImplementedException();
    }
}
