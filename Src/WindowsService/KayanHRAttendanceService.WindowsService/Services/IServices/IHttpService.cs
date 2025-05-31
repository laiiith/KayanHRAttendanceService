using KayanHRAttendanceService.Entities.General;

namespace KayanHRAttendanceService.WindowsService.Services.IServices;

public interface IHttpService
{
    Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true);
}
