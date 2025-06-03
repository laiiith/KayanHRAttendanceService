using KayanHRAttendanceService.Application.Interfaces.Services;

namespace KayanHRAttendanceService.Infrastructure.Services;

public class SyncAttendanceData(IDataPusherService dataPusherService, IAttendanceFetcherService attendanceFetcherService) : ISyncAttendanceData
{
    public async Task SyncData()
    {
        await attendanceFetcherService.FetchAsync();
        await dataPusherService.PushAsync();
    }
}