using KayanHRAttendanceService.Domain.Entities.Services;

namespace KayanHRAttendanceService.Application.Interfaces;

public interface IHttpService
{
    Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true);
}
