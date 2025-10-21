using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorCrudDemo.Web.Components.Dashboard;
using BlazorCrudDemo.Web.Models;
using Microsoft.JSInterop;

namespace BlazorCrudDemo.Web.Pages.Dashboard
{
    public partial class Index : ComponentBase, IDisposable
    {
        [Inject] private ILogger<Index> Logger { get; set; } = default!;

        // Optional: Inject services when you have real data
        // [Inject] private IDashboardService DashboardService { get; set; } = default!;
        // [Inject] private IActivityService ActivityService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private bool isLoading = true;
        private bool hasError = false;
#pragma warning disable CS0414 // Field is assigned but never used
        private string? errorMessage;
#pragma warning restore CS0414
        private bool showSidebar = true;
        
        // Data models
        private DashboardStats stats = new();
        private List<ActivityItem> recentActivities = new();
        
        // Last refresh timestamp
        private DateTime lastRefreshed;
        
        // Auto-refresh timer
        private System.Threading.Timer? autoRefreshTimer;
        private const int AUTO_REFRESH_INTERVAL_MS = 300000; // 5 minutes
        
        // Cancellation token for cleanup
        private System.Threading.CancellationTokenSource? cancellationTokenSource;

        protected override async Task OnInitializedAsync()
        {
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
                        Title = "Admin logged in",
                        Description = "Administrator access from 192.168.1.100",
                        Timestamp = DateTime.UtcNow.AddHours(-3),
                        Link = "/admin/activity"
                    },
                    new()
                    {
                        Type = "product.created",
                        Title = "New product added",
                        Description = "Bluetooth Speaker Mini added to Audio category",
                        Timestamp = DateTime.UtcNow.AddHours(-5),
                        Link = "/products/101"
                    },
                    new()
                    {
                        Type = "order.shipped",
                        Title = "Order shipped",
                        Description = "Order #ORD-2024-0145 has been marked as shipped",
                        Timestamp = DateTime.UtcNow.AddHours(-8),
                        Link = "/orders/145"
                    },
                    new()
                    {
                        Type = "category.updated",
                        Title = "Category updated",
                        Description = "Electronics category description and SEO metadata updated",
                        Timestamp = DateTime.UtcNow.AddDays(-1),
                        Link = "/categories/electronics"
                    }
                };

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