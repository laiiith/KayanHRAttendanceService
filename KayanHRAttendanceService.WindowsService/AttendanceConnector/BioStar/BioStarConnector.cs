using KayanHRAttendanceService.Entities.Sqlite;

namespace KayanHRAttendanceService.WindowsService.AttendanceConnector.BioStar;

public class BioStarConnector : AttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceAsync()
    {
        throw new NotImplementedException();
    }
}
