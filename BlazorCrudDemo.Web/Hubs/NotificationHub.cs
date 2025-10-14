using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Web.Hubs;

/// <summary>
/// SignalR hub for real-time communication between server and clients.
/// </summary>
public class NotificationHub : Hub<INotificationClient>
{
    private readonly ILogger<NotificationHub> _logger;

    /// <summary>
    /// Initializes a new instance of the NotificationHub class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);

        // Send welcome message to the newly connected client
        await Clients.Caller.ReceiveMessage(new NotificationMessage
        {
            Type = NotificationType.Info,
            Title = "Connected",
            Message = "Successfully connected to notification hub",
            Timestamp = DateTime.UtcNow
        });

        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);

        if (exception != null)
        {
            _logger.LogWarning("Client disconnected with error: {Error}", exception.Message);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Sends a notification to all connected clients.
    /// </summary>
    /// <param name="type">The notification type.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendNotificationToAllAsync(NotificationType type, string title, string message)
    {
        _logger.LogInformation("Sending notification to all clients: {Title} - {Message}", title, message);

        await Clients.All.ReceiveMessage(new NotificationMessage
        {
            Type = type,
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sends a notification to a specific user group.
    /// </summary>
    /// <param name="groupName">The group name.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendNotificationToGroupAsync(string groupName, NotificationType type, string title, string message)
    {
        _logger.LogInformation("Sending notification to group '{GroupName}': {Title} - {Message}", groupName, title, message);

        await Clients.Group(groupName).ReceiveMessage(new NotificationMessage
        {
            Type = type,
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Sends a notification to the calling client only.
    /// </summary>
    /// <param name="type">The notification type.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendNotificationToCallerAsync(NotificationType type, string title, string message)
    {
        _logger.LogDebug("Sending notification to caller: {Title} - {Message}", title, message);

        await Clients.Caller.ReceiveMessage(new NotificationMessage
        {
            Type = type,
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifies clients about a product creation.
    /// </summary>
    /// <param name="productDto">The created product.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task NotifyProductCreatedAsync(ProductDto productDto)
    {
        _logger.LogInformation("Notifying clients about product creation: {ProductName}", productDto.Name);

        await Clients.All.ReceiveProductUpdate(new ProductUpdateMessage
        {
            Action = UpdateAction.Created,
            Product = productDto,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifies clients about a product update.
    /// </summary>
    /// <param name="productDto">The updated product.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task NotifyProductUpdatedAsync(ProductDto productDto)
    {
        _logger.LogInformation("Notifying clients about product update: {ProductName}", productDto.Name);

        await Clients.All.ReceiveProductUpdate(new ProductUpdateMessage
        {
            Action = UpdateAction.Updated,
            Product = productDto,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifies clients about a product deletion.
    /// </summary>
    /// <param name="productId">The deleted product ID.</param>
    /// <param name="productName">The deleted product name.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task NotifyProductDeletedAsync(int productId, string productName)
    {
        _logger.LogInformation("Notifying clients about product deletion: {ProductName} (ID: {ProductId})", productName, productId);

        await Clients.All.ReceiveProductUpdate(new ProductUpdateMessage
        {
            Action = UpdateAction.Deleted,
            Product = new ProductDto { Id = productId, Name = productName },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifies clients about a category creation.
    /// </summary>
    /// <param name="categoryDto">The created category.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task NotifyCategoryCreatedAsync(CategoryDto categoryDto)
    {
        _logger.LogInformation("Notifying clients about category creation: {CategoryName}", categoryDto.Name);

        await Clients.All.ReceiveCategoryUpdate(new CategoryUpdateMessage
        {
            Action = UpdateAction.Created,
            Category = categoryDto,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifies clients about a category update.
    /// </summary>
    /// <param name="categoryDto">The updated category.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task NotifyCategoryUpdatedAsync(CategoryDto categoryDto)
    {
        _logger.LogInformation("Notifying clients about category update: {CategoryName}", categoryDto.Name);

        await Clients.All.ReceiveCategoryUpdate(new CategoryUpdateMessage
        {
            Action = UpdateAction.Updated,
            Category = categoryDto,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifies clients about a category deletion.
    /// </summary>
    /// <param name="categoryId">The deleted category ID.</param>
    /// <param name="categoryName">The deleted category name.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task NotifyCategoryDeletedAsync(int categoryId, string categoryName)
    {
        _logger.LogInformation("Notifying clients about category deletion: {CategoryName} (ID: {CategoryId})", categoryName, categoryId);

        await Clients.All.ReceiveCategoryUpdate(new CategoryUpdateMessage
        {
            Action = UpdateAction.Deleted,
            Category = new CategoryDto { Id = categoryId, Name = categoryName },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifies clients about low stock products.
    /// </summary>
    /// <param name="lowStockProducts">The products with low stock.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task NotifyLowStockAsync(IEnumerable<ProductDto> lowStockProducts)
    {
        _logger.LogInformation("Notifying clients about {Count} low stock products", lowStockProducts.Count());

        await Clients.All.ReceiveLowStockAlert(new LowStockAlertMessage
        {
            LowStockProducts = lowStockProducts.ToList(),
            AlertThreshold = 10, // This could be configurable
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notifies clients about system statistics updates.
    /// </summary>
    /// <param name="productStats">The product statistics.</param>
    /// <param name="categoryStats">The category statistics.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task NotifyStatisticsUpdateAsync(
        (int TotalProducts, int ActiveProducts, int OutOfStockProducts, decimal AveragePrice) productStats,
        (int TotalCategories, int ActiveCategories, int CategoriesWithProducts, int EmptyCategories) categoryStats)
    {
        _logger.LogDebug("Notifying clients about statistics update");

        await Clients.All.ReceiveStatisticsUpdate(new StatisticsUpdateMessage
        {
            ProductStatistics = productStats,
            CategoryStatistics = categoryStats,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Joins a user group for targeted notifications.
    /// </summary>
    /// <param name="groupName">The group name to join.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task JoinGroupAsync(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined group '{GroupName}'", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leaves a user group.
    /// </summary>
    /// <param name="groupName">The group name to leave.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LeaveGroupAsync(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left group '{GroupName}'", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Sends a heartbeat to keep the connection alive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendHeartbeatAsync()
    {
        await Clients.Caller.ReceiveHeartbeat(new HeartbeatMessage
        {
            Timestamp = DateTime.UtcNow,
            ServerTime = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Client-side interface for SignalR hub communication.
/// </summary>
public interface INotificationClient
{
    /// <summary>
    /// Receives a notification message.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReceiveMessage(NotificationMessage message);

    /// <summary>
    /// Receives a product update notification.
    /// </summary>
    /// <param name="update">The product update message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReceiveProductUpdate(ProductUpdateMessage update);

    /// <summary>
    /// Receives a category update notification.
    /// </summary>
    /// <param name="update">The category update message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReceiveCategoryUpdate(CategoryUpdateMessage update);

    /// <summary>
    /// Receives a low stock alert.
    /// </summary>
    /// <param name="alert">The low stock alert message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReceiveLowStockAlert(LowStockAlertMessage alert);

    /// <summary>
    /// Receives statistics update.
    /// </summary>
    /// <param name="update">The statistics update message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReceiveStatisticsUpdate(StatisticsUpdateMessage update);

    /// <summary>
    /// Receives a heartbeat message.
    /// </summary>
    /// <param name="heartbeat">The heartbeat message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReceiveHeartbeat(HeartbeatMessage heartbeat);
}

/// <summary>
/// Base class for notification messages.
/// </summary>
public abstract class BaseMessage
{
    /// <summary>
    /// The timestamp when the message was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Notification message for general notifications.
/// </summary>
public class NotificationMessage : BaseMessage
{
    /// <summary>
    /// The notification type.
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// The notification title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Product update message.
/// </summary>
public class ProductUpdateMessage : BaseMessage
{
    /// <summary>
    /// The update action.
    /// </summary>
    public UpdateAction Action { get; set; }

    /// <summary>
    /// The product data.
    /// </summary>
    public ProductDto Product { get; set; } = new();
}

/// <summary>
/// Category update message.
/// </summary>
public class CategoryUpdateMessage : BaseMessage
{
    /// <summary>
    /// The update action.
    /// </summary>
    public UpdateAction Action { get; set; }

    /// <summary>
    /// The category data.
    /// </summary>
    public CategoryDto Category { get; set; } = new();
}

/// <summary>
/// Low stock alert message.
/// </summary>
public class LowStockAlertMessage : BaseMessage
{
    /// <summary>
    /// The products with low stock.
    /// </summary>
    public List<ProductDto> LowStockProducts { get; set; } = new();

    /// <summary>
    /// The alert threshold.
    /// </summary>
    public int AlertThreshold { get; set; }
}

/// <summary>
/// Statistics update message.
/// </summary>
public class StatisticsUpdateMessage : BaseMessage
{
    /// <summary>
    /// The product statistics.
    /// </summary>
    public (int TotalProducts, int ActiveProducts, int OutOfStockProducts, decimal AveragePrice) ProductStatistics { get; set; }

    /// <summary>
    /// The category statistics.
    /// </summary>
    public (int TotalCategories, int ActiveCategories, int CategoriesWithProducts, int EmptyCategories) CategoryStatistics { get; set; }
}

/// <summary>
/// Heartbeat message for connection monitoring.
/// </summary>
public class HeartbeatMessage : BaseMessage
{
    /// <summary>
    /// The server timestamp.
    /// </summary>
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Notification types.
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Update action types.
/// </summary>
public enum UpdateAction
{
    Created,
    Updated,
    Deleted
}
