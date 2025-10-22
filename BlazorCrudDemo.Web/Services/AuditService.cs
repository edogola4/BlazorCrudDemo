using Microsoft.EntityFrameworkCore;
using BlazorCrudDemo.Data.Models;
using BlazorCrudDemo.Data.Contexts;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace BlazorCrudDemo.Web.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuditService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AuditService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActivityAsync(string action, string entityType, int? entityId,
                                         string? oldValues, string? newValues, string? changes,
                                         string userId, string? userName, string? ipAddress, string? userAgent)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    Changes = changes,
                    UserId = userId,
                    UserName = userName,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit activity: {Action} on {EntityType}", action, entityType);
            }
        }

        public async Task LogUserActivityAsync(string userId, string activityType, string? description,
                                             string? details, int? entityId, string? entityType,
                                             string? ipAddress, string? userAgent)
        {
            try
            {
                var activity = new UserActivity
                {
                    ActivityType = activityType,
                    Description = description,
                    Details = details,
                    EntityId = entityId,
                    EntityType = entityType,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user activity: {ActivityType} for user {UserId}", activityType, userId);
            }
        }

        public async Task LogLoginAsync(string userId, string? ipAddress, string? userAgent, bool isSuccessful = true, string? failureReason = null)
        {
            try
            {
                var loginHistory = new LoginHistory
                {
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsSuccessful = isSuccessful,
                    FailureReason = failureReason
                };

                _context.LoginHistory.Add(loginHistory);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging login for user {UserId}", userId);
            }
        }

        public async Task LogLogoutAsync(string userId, TimeSpan? sessionDuration = null)
        {
            try
            {
                // Find the latest login record for this user that doesn't have a logout time
                var latestLogin = await _context.LoginHistory
                    .Where(l => l.UserId == userId && l.LogoutTime == null)
                    .OrderByDescending(l => l.LoginTime)
                    .FirstOrDefaultAsync();

                if (latestLogin != null)
                {
                    latestLogin.LogoutTime = DateTime.UtcNow;
                    latestLogin.SessionDuration = sessionDuration ?? (DateTime.UtcNow - latestLogin.LoginTime);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging logout for user {UserId}", userId);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50, string? userId = null,
                                                          string? entityType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                    query = query.Where(a => a.UserId == userId);

                if (!string.IsNullOrEmpty(entityType))
                    query = query.Where(a => a.EntityType == entityType);

                if (startDate.HasValue)
                    query = query.Where(a => a.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.Timestamp <= endDate.Value);

                return await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs");
                return new List<AuditLog>();
            }
        }

        public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int page = 1, int pageSize = 50)
        {
            try
            {
                return await _context.UserActivities
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user activities for user {UserId}", userId);
                return new List<UserActivity>();
            }
        }

        public async Task<List<LoginHistory>> GetLoginHistoryAsync(string userId, int page = 1, int pageSize = 50)
        {
            try
            {
                return await _context.LoginHistory
                    .Where(l => l.UserId == userId)
                    .OrderByDescending(l => l.LoginTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login history for user {UserId}", userId);
                return new List<LoginHistory>();
            }
        }

        public async Task<int> GetAuditLogCountAsync(string? userId = null, string? entityType = null,
                                                   DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                    query = query.Where(a => a.UserId == userId);

                if (!string.IsNullOrEmpty(entityType))
                    query = query.Where(a => a.EntityType == entityType);

                if (startDate.HasValue)
                    query = query.Where(a => a.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.Timestamp <= endDate.Value);

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit log count");
                return 0;
            }
        }

        // Helper method to get current user information
        private async Task<(string UserId, string UserName)> GetCurrentUserInfoAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
                if (user != null)
                {
                    return (user.Id, $"{user.FirstName} {user.LastName}".Trim());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info");
            }

            return ("system", "System");
        }

        // Helper method to get client IP address
        private string GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }

        // Helper method to get user agent
        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "unknown";
        }
    }
}
