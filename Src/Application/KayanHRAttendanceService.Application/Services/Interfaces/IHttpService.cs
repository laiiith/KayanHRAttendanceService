namespace KayanHRAttendanceService.Application.Services.Interfaces;

public interface IHttpService
{
    Task<T> SendAsync<T>(KayanHRAttendanceService.Domain.Entities.General.APIRequest apiRequest, bool withBearer = true);
}
