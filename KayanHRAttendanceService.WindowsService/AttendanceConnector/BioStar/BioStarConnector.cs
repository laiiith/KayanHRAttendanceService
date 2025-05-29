using KayanHRAttendanceService.WindowsService.Storage.Models;

namespace KayanHRAttendanceService.WindowsService.AttendanceConnector.BioStar;

public class BioStarConnector : AttendanceConnector, IAttendanceConnector
{
    public Task<List<AttendanceRecord>> FetchAttendanceAsync()
    {
        throw new NotImplementedException();
    }
}
