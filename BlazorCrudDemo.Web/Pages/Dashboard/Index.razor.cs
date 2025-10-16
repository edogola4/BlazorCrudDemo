using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorCrudDemo.Web.Components.Dashboard;
using BlazorCrudDemo.Web.Models;
using BlazorCrudDemo.Web.Pages.Dashboard;

namespace BlazorCrudDemo.Web.Pages.Dashboard
{
    public partial class Index : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private ILogger<Index> Logger { get; set; } = default!;

        private bool isLoading = true;
        private bool hasError = false;
        
        // Data models
        private DashboardStats stats = new();
        private List<ActivityItem> recentActivities = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadDashboardData();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading dashboard data");
                hasError = true;
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task LoadDashboardData()
        {
            // Simulate API calls with mock data
            await Task.WhenAll(
                LoadStatsAsync(),
                LoadRecentActivitiesAsync()
            );
        }

        private async Task LoadStatsAsync()
        {
            // Simulate API call
            await Task.Delay(200);
            
            stats = new DashboardStats
            {
                TotalProducts = 1428,
                ProductsTrend = 12.5m,
                TotalCategories = 24,
                CategoriesTrend = 4.2m,
                LowStockItems = 18,
                LowStockTrend = -8.3m,
                TotalInventoryValue = 125487.65m,
                InventoryValueTrend = 5.7m
            };
        }

        private async Task LoadRecentActivitiesAsync()
        {
            // Simulate API call
            await Task.Delay(400);
            
            recentActivities = new List<ActivityItem>
            {
                new() 
                { 
                    Type = "product.added", 
                    Title = "New product added", 
                    Description = "Wireless Earbuds Pro was added to Electronics",
                    Timestamp = DateTime.UtcNow.AddMinutes(-15),
                    Link = "/products/123"
                },
                new() 
                { 
                    Type = "product.updated", 
                    Title = "Product updated", 
                    Description = "Smart Watch X1 price was updated to $199.99",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Link = "/products/456"
                },
                new() 
                { 
                    Type = "product.low-stock", 
                    Title = "Low stock alert", 
                    Description = "Only 3 units left for Wireless Charger",
                    Timestamp = DateTime.UtcNow.AddHours(-5),
                    Link = "/products/789"
                },
                new() 
                { 
                    Type = "category.updated", 
                    Title = "Category updated", 
                    Description = "Home & Kitchen category was modified",
                    Timestamp = DateTime.UtcNow.AddDays(-1),
                    Link = "/categories/12"
                },
                new() 
                { 
                    Type = "user.login", 
                    Title = "User login", 
                    Description = "Admin logged in from 192.168.1.1",
                    Timestamp = DateTime.UtcNow.AddDays(-1).AddHours(-2)
                }
            };
        }

        private async Task RefreshDashboard()
        {
            isLoading = true;
            hasError = false;
            StateHasChanged();

            try
            {
                await LoadDashboardData();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error refreshing dashboard");
                hasError = true;
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
    }

    public class QuickAction
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
