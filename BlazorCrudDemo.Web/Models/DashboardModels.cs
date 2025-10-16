using System;
using System.Collections.Generic;

namespace BlazorCrudDemo.Web.Models
{
    public enum TrendDirection
    {
        Up,
        Down
    }

    public class ActivityItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Link { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class QuickAction
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class DashboardStats
    {
        public int TotalProducts { get; set; }
        public decimal ProductsTrend { get; set; }
        public int TotalCategories { get; set; }
        public decimal CategoriesTrend { get; set; }
        public int LowStockItems { get; set; }
        public decimal LowStockTrend { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public decimal InventoryValueTrend { get; set; }
    }

    public class CategoryDistribution
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StockLevel
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class PriceRange
    {
        public string Range { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class SalesData
    {
        public string Date { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class LowStockItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int AlertThreshold { get; set; }
    }
}
