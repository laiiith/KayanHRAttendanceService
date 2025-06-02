using KayanHRAttendanceService.Application.Interfaces.Services;

namespace KayanHRAttendanceService.Infrastructure.Services;

public class SyncAttendanceData(IDataPusherService dataPusherService, IAttendanceFetcherService attendanceFetcherService) : ISyncAttendanceData
{
}
