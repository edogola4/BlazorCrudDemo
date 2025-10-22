using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorCrudDemo.Web.Components.Dashboard;
using BlazorCrudDemo.Web.Models;
using BlazorCrudDemo.Web.Services;
using BlazorCrudDemo.Shared.DTOs;

namespace BlazorCrudDemo.Web.Pages.Dashboard
{
    // [Authorize] - Removed to disable authentication
    public partial class Index : ComponentBase, IDisposable
    {
        [Inject] private ILogger<Index> Logger { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        // [Inject] private IAuthenticationService AuthenticationService { get; set; } = default!;
        [Inject] private IAuditService AuditService { get; set; } = default!;

        private bool isLoading = true;
        private bool hasError = false;
        private string searchQuery = string.Empty;
        private string selectedFilter = "all";
        private string? errorMessage;
        private DashboardStats stats = new();
        private List<ActivityItem> recentActivities = new();
        // private ApplicationUserDto? currentUser; - Removed since auth is disabled

        private IQueryable<ActivityItem> FilteredActivities => recentActivities.AsQueryable()
            .Where(a => string.IsNullOrEmpty(searchQuery) ||
                   a.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                   a.Description != null && a.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                   a.Type.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            .Where(a => selectedFilter == "all" || a.Type.Contains(selectedFilter.Replace("s", "").Replace("orders", "order").Replace("categories", "category")));

        // Last refresh timestamp
        private DateTime lastRefreshed;

        // Auto-refresh timer
        private System.Threading.Timer? autoRefreshTimer;
        private const int AUTO_REFRESH_INTERVAL_MS = 300000; // 5 minutes

        // Cancellation token for cleanup
        private System.Threading.CancellationTokenSource? cancellationTokenSource;

        protected override async Task OnInitializedAsync()
        {
            // currentUser = await AuthenticationService.GetCurrentUserAsync();

            // if (currentUser == null)
            // {
            //     Navigation.NavigateTo("/auth/login");
            //     return;
            // }

            // Log dashboard access
            // await AuditService.LogUserActivityAsync(
            //     currentUser.Id,
            //     "DASHBOARD_VIEW",
            //     "User accessed dashboard",
            //     null,
            //     null,
            //     "Dashboard",
            //     GetClientIpAddress(),
            //     GetUserAgent());

            cancellationTokenSource = new System.Threading.CancellationTokenSource();

            try
            {
                await LoadDashboardDataAsync();

                // Optional: Enable auto-refresh
                // StartAutoRefresh();
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Dashboard initialization was cancelled");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading dashboard data");
                hasError = true;
                errorMessage = "Failed to load dashboard data. Please try again.";
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            if (cancellationTokenSource?.Token.IsCancellationRequested == true)
                return;

            try
            {
                isLoading = true; // Ensure loading state is set
                await InvokeAsync(StateHasChanged); // Force UI update to show loading

                // Load data in parallel for better performance
                await Task.WhenAll(
                    LoadStatsAsync(),
                    LoadRecentActivitiesAsync()
                );

                lastRefreshed = DateTime.Now;
                hasError = false;
                errorMessage = null;
                isLoading = false; // Set loading to false after data loads
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in LoadDashboardDataAsync");
                isLoading = false; // Ensure loading is set to false even on error
                hasError = true;
                errorMessage = "Failed to load dashboard data. Please try again.";
                throw;
            }
            finally
            {
                await InvokeAsync(StateHasChanged); // Force final UI update
            }
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                // Simulate API call with realistic delay
                await Task.Delay(200, cancellationTokenSource?.Token ?? default);

                // TODO: Replace with actual service call
                // stats = await DashboardService.GetStatsAsync();

                // Generate stats based on user role
                // var isAdmin = currentUser?.Roles.Contains("Admin") == true;

                stats = new DashboardStats
                {
                    TotalProducts = 1247,
                    ProductsTrend = 12.5m,
                    TotalCategories = 34,
                    CategoriesTrend = 8.2m,
                    LowStockItems = 23,
                    LowStockTrend = -15.3m,
                    TotalInventoryValue = 89456.78m,
                    InventoryValueTrend = 18.7m,
                    TotalOrders = 342,
                    OrdersTrend = 25.1m,
                    TotalRevenue = 45678.90m,
                    RevenueTrend = 22.4m
                };

                // Add admin-specific stats
                // if (isAdmin)
                // {
                //     // Add admin dashboard stats here if needed
                // }

                Logger.LogInformation("Dashboard stats loaded successfully");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading dashboard stats");
                throw;
            }
        }

        private async Task LoadRecentActivitiesAsync()
        {
            try
            {
                // Simulate API call
                await Task.Delay(400, cancellationTokenSource?.Token ?? default);

                // TODO: Replace with actual service call
                // recentActivities = await ActivityService.GetRecentAsync(limit: 5);

                // Generate activities based on user role
                // var isAdmin = currentUser?.Roles.Contains("Admin") == true;

                recentActivities = new List<ActivityItem>
                {
                    new()
                    {
                        Type = "order.created",
                        Title = "New order received",
                        Description = "Order #ORD-2024-0156 for $299.99 from Electronics category",
                        Timestamp = DateTime.UtcNow.AddMinutes(-8),
                        Link = "/orders/156"
                    },
                    new()
                    {
                        Type = "product.updated",
                        Title = "Product price updated",
                        Description = "Wireless Earbuds Pro price changed from $199.99 to $179.99",
                        Timestamp = DateTime.UtcNow.AddMinutes(-23),
                        Link = "/products/789"
                    },
                    new()
                    {
                        Type = "inventory.alert",
                        Title = "Low stock alert",
                        Description = "Smart Watch X1 inventory dropped below threshold (3 units remaining)",
                        Timestamp = DateTime.UtcNow.AddMinutes(-45),
                        Link = "/products/456"
                    },
                    new()
                    {
                        Type = "category.created",
                        Title = "New category added",
                        Description = "Created 'Smart Home' category with 12 products",
                        Timestamp = DateTime.UtcNow.AddHours(-2),
                        Link = "/categories/smart-home"
                    },
                    new()
                    {
                        Type = "user.login",
                        Title = "User logged in",
                        Description = $"Welcome back, User!", // Removed currentUser reference
                        Timestamp = DateTime.UtcNow.AddHours(-3),
                        Link = "/account/profile"
                    }
                };

                // Add admin-specific activities
                // if (isAdmin)
                // {
                //     recentActivities.Add(new ActivityItem
                //     {
                //         Type = "system.alert",
                //         Title = "System maintenance scheduled",
                //         Description = "Database maintenance scheduled for tonight at 2:00 AM",
                //         Timestamp = DateTime.UtcNow.AddHours(-1),
                //         Link = "/admin/system"
                //     });
                // }

                Logger.LogInformation("Recent activities loaded: {Count} items", recentActivities.Count);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading recent activities");
                throw;
            }
        }

        private async Task RefreshDashboard()
        {
            if (isLoading)
            {
                Logger.LogWarning("Dashboard refresh already in progress");
                return;
            }

            isLoading = true;
            hasError = false;
            errorMessage = null;

            // Force UI update to show loading state
            await InvokeAsync(StateHasChanged);

            try
            {
                await LoadDashboardDataAsync();
                Logger.LogInformation("Dashboard refreshed successfully");

                // Refresh charts after data update
                await InvokeAsync(async () => await JS.InvokeVoidAsync("refreshCharts"));
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Dashboard refresh was cancelled");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error refreshing dashboard");
                hasError = true;
            }
            finally
            {
                isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private void NavigateToAddProduct()
        {
            Navigation.NavigateTo("/products/add");
        }

        private void NavigateToAddCategory()
        {
            Navigation.NavigateTo("/categories/add");
        }

        private void NavigateToReports()
        {
            Navigation.NavigateTo("/reports");
        }

        private void NavigateToSettings()
        {
            Navigation.NavigateTo("/account/settings");
        }

        private void NavigateToHome()
        {
            Navigation.NavigateTo("/");
        }

        private void NavigateToInventory()
        {
            Navigation.NavigateTo("/products");
        }

        private void ClearSearch()
        {
            searchQuery = string.Empty;
            selectedFilter = "all";
        }

        private string GetActivityIcon(string activityType)
        {
            return activityType switch
            {
                "order.created" => "shopping-cart",
                "order.shipped" => "truck",
                "product.created" => "box",
                "product.updated" => "edit",
                "category.created" => "folder",
                "category.updated" => "folder-open",
                "inventory.alert" => "exclamation-triangle",
                "user.login" => "user",
                "system.alert" => "cog",
                _ => "circle"
            };
        }

        private string GetActivityBadgeType(string activityType)
        {
            return activityType switch
            {
                "order.created" => "success",
                "order.shipped" => "primary",
                "product.created" => "info",
                "product.updated" => "warning",
                "category.created" => "success",
                "category.updated" => "info",
                "inventory.alert" => "danger",
                "user.login" => "secondary",
                "system.alert" => "warning",
                _ => "secondary"
            };
        }

        private string GetActivityBadgeText(string activityType)
        {
            return activityType switch
            {
                "order.created" => "Order",
                "order.shipped" => "Shipped",
                "product.created" => "Product",
                "product.updated" => "Updated",
                "category.created" => "Category",
                "category.updated" => "Updated",
                "inventory.alert" => "Alert",
                "user.login" => "Login",
                "system.alert" => "System",
                _ => "Activity"
            };
        }

        private string GetRelativeTime(DateTime timestamp)
        {
            var now = DateTime.UtcNow;
            var difference = now - timestamp;

            return difference.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)difference.TotalMinutes} minute{((int)difference.TotalMinutes == 1 ? "" : "s")} ago",
                < 1440 => $"{(int)difference.TotalHours} hour{((int)difference.TotalHours == 1 ? "" : "s")} ago",
                < 10080 => $"{(int)difference.TotalDays} day{((int)difference.TotalDays == 1 ? "" : "s")} ago",
                _ => timestamp.ToString("MMM dd, yyyy")
            };
        }

        private string GetClientIpAddress()
        {
            // In a real implementation, this would get the actual client IP
            return "unknown";
        }

        private string GetUserAgent()
        {
            // In a real implementation, this would get the actual user agent
            return "Blazor Application";
        }

        #region Auto-Refresh Feature (Optional)

        private void StartAutoRefresh()
        {
            autoRefreshTimer = new System.Threading.Timer(
                async _ => await AutoRefreshCallback(),
                null,
                AUTO_REFRESH_INTERVAL_MS,
                AUTO_REFRESH_INTERVAL_MS
            );

            Logger.LogInformation("Auto-refresh started with interval: {Interval}ms", AUTO_REFRESH_INTERVAL_MS);
        }

        private async Task AutoRefreshCallback()
        {
            try
            {
                Logger.LogDebug("Auto-refresh triggered");
                await InvokeAsync(async () => await RefreshDashboard());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during auto-refresh");
            }
        }

        private void StopAutoRefresh()
        {
            autoRefreshTimer?.Dispose();
            autoRefreshTimer = null;
            Logger.LogInformation("Auto-refresh stopped");
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            try
            {
                // Cancel any pending operations
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();

                // Stop auto-refresh timer
                StopAutoRefresh();

                Logger.LogInformation("Dashboard component disposed");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error disposing dashboard component");
            }
        }

        #endregion
    }

    #region Models (Move these to separate files in production)

    #endregion
}
