using KayanHRAttendanceService.Domain.Entities.Services;

namespace KayanHRAttendanceService.Application.Interfaces;

public interface IHttpService
{
    Task<ApiResponse<TResponse>> SendAsync<TResponse>(APIRequest apiRequest);
}