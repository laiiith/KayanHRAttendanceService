using KayanHRAttendanceService.Application.Services.Interfaces;

namespace KayanHRAttendanceService.Infrastructure.Services;

public class SyncAttendanceData(IAttendanceFetcherService fetcherService, IDataPusherService pusherService)
{
}
