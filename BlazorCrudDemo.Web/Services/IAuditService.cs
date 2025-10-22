using BlazorCrudDemo.Data.Models;

namespace BlazorCrudDemo.Web.Services
{
    public interface IAuditService
    {
        Task LogActivityAsync(string action, string entityType, int? entityId,
                            string? oldValues, string? newValues, string? changes,
                            string userId, string? userName, string? ipAddress, string? userAgent);

        Task LogUserActivityAsync(string userId, string activityType, string? description,
                                string? details, int? entityId, string? entityType,
                                string? ipAddress, string? userAgent);

        Task LogLoginAsync(string userId, string? ipAddress, string? userAgent, bool isSuccessful = true, string? failureReason = null);

        Task LogLogoutAsync(string userId, TimeSpan? sessionDuration = null);

        Task<List<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50, string? userId = null,
                                             string? entityType = null, DateTime? startDate = null, DateTime? endDate = null);

        Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int page = 1, int pageSize = 50);

        Task<List<LoginHistory>> GetLoginHistoryAsync(string userId, int page = 1, int pageSize = 50);

        Task<int> GetAuditLogCountAsync(string? userId = null, string? entityType = null,
                                      DateTime? startDate = null, DateTime? endDate = null);
    }
}
