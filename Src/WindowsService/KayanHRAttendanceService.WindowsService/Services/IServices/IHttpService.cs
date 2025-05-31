
namespace KayanHRAttendanceService.WindowsService.Services.IServices;

public interface IHttpService
{
    Task<T> SendAsync<T>(KayanHRAttendanceService.Domain.Entities.General.APIRequest apiRequest, bool withBearer = true);
}
