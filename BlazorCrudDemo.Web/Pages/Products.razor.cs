using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorCrudDemo.Web.Pages;

public partial class Products : IDisposable
{
    [Inject] private IProductService ProductService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    // View State
    private string ViewMode { get; set; } = "table";
    private string SearchTerm { get; set; } = "";
    private int? SelectedCategoryId { get; set; }
    private string SortBy { get; set; } = "name-asc";
    private System.Timers.Timer? _searchTimer;

    // Pagination
    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 10;
    private int TotalCount { get; set; }
    private int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // Data
    private List<ProductDto> _products { get; set; } = new();
    private List<CategoryDto> Categories { get; set; } = new();
    private List<int> SelectedProducts { get; set; } = new();

    // Public property for Razor markup access
    public List<ProductDto> ProductsList => _products;

    // UI State
    private bool IsLoading { get; set; } = true;
    private bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || SelectedCategoryId.HasValue;

    // Modals
    private bool ShowDeleteModal { get; set; }
    private bool ShowBulkDeleteModal { get; set; }
    private ProductDto? ProductToDelete { get; set; }
    private bool IsDeleting { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Load categories first
            await LoadCategoriesAsync();

            // Load initial products
            await LoadProductsAsync();

            // Setup keyboard shortcuts
            await SetupKeyboardShortcuts();
        }
        catch (Exception ex)
        {
            await ShowToast("Initialization Error", $"Failed to initialize products page: {ex.Message}", "danger");
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            Categories = (await CategoryService.GetCategoriesAsync()).ToList();
        }
        catch (Exception ex)
        {
            await ShowToast("Error loading categories", ex.Message, "danger");
        }
    }

    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        StateHasChanged();

        try
        {
            var result = await ProductService.GetProductsAsync(
                categoryId: SelectedCategoryId,
                searchTerm: SearchTerm,
                pageNumber: CurrentPage,
                pageSize: PageSize,
                sortBy: ParseSortBy(SortBy));

            _products = result.Items.ToList();
            TotalCount = result.TotalCount;
        }
        catch (Exception ex)
        {
            await ShowToast("Error loading products", ex.Message, "danger");
            _products.Clear();
            TotalCount = 0;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task SetupKeyboardShortcuts()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("setupProductsKeyboardShortcuts", DotNetObjectReference.Create(this));
        }
        catch (Exception ex)
        {
            // Silently fail if JS interop isn't available
            Console.WriteLine($"Failed to setup keyboard shortcuts: {ex.Message}");
        }
    }

    private void ClearSearch()
    {
        SearchTerm = "";
        CurrentPage = 1;
        SelectedProducts.Clear();
        _ = LoadProductsAsync();
    }

    private void ClearAllFilters()
    {
        SearchTerm = "";
        SelectedCategoryId = null;
        SortBy = "name-asc";
        CurrentPage = 1;
        SelectedProducts.Clear();
        _ = LoadProductsAsync();
    }

    private async Task OnSearchInput()
    {
        // Debounce search input
        _searchTimer?.Dispose();
        _searchTimer = new System.Timers.Timer(300);
        _searchTimer.Elapsed += async (sender, args) =>
        {
            _searchTimer?.Dispose();
            _searchTimer = null;

            await InvokeAsync(async () =>
            {
                CurrentPage = 1;
                SelectedProducts.Clear();
                await LoadProductsAsync();
            });
        };
        _searchTimer.Start();
    }

    private void NavigateToCreate()
    {
        Navigation.NavigateTo("/products/create");
    }

    private void ViewProduct(int productId)
    {
        Navigation.NavigateTo($"/products/{productId}");
    }

    private void EditProduct(int productId)
    {
        Navigation.NavigateTo($"/products/{productId}/edit");
    }

    private void ShowDeleteConfirmation(ProductDto product)
    {
        ProductToDelete = product;
        ShowDeleteModal = true;
        StateHasChanged();
    }

    private void HideDeleteModal()
    {
        ShowDeleteModal = false;
        ProductToDelete = null;
        StateHasChanged();
    }

    private async Task ConfirmDelete()
    {
        if (ProductToDelete == null) return;

        IsDeleting = true;
        StateHasChanged();

        try
        {
            await ProductService.DeleteProductAsync(ProductToDelete.Id);
            await ShowToast("Product Deleted", $"{ProductToDelete.Name} has been deleted successfully.", "success");
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            await ShowToast("Delete Failed", ex.Message, "danger");
        }
        finally
        {
            IsDeleting = false;
            HideDeleteModal();
        }
    }

    private void ShowBulkDeleteConfirmation()
    {
        if (!SelectedProducts.Any())
        {
            _ = ShowToast("No Selection", "Please select products to delete.", "warning");
            return;
        }

        ShowBulkDeleteModal = true;
        StateHasChanged();
    }

    private void HideBulkDeleteModal()
    {
        ShowBulkDeleteModal = false;
        StateHasChanged();
    }

    private async Task ConfirmBulkDelete()
    {
        if (!SelectedProducts.Any()) return;

        IsDeleting = true;
        StateHasChanged();

        try
        {
            var deletedCount = 0;
            var errors = new List<string>();

            foreach (var productId in SelectedProducts)
            {
                try
                {
                    var success = await ProductService.DeleteProductAsync(productId);
                    if (success)
                        deletedCount++;
                    else
                        errors.Add($"Failed to delete product ID {productId}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Error deleting product ID {productId}: {ex.Message}");
                }
            }

            if (deletedCount > 0)
            {
                await ShowToast("Products Deleted", $"{deletedCount} products have been deleted successfully.", "success");
            }

            if (errors.Any())
            {
                await ShowToast("Some Deletions Failed", string.Join("\n", errors), "warning");
            }

            SelectedProducts.Clear();
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            await ShowToast("Bulk Delete Failed", ex.Message, "danger");
        }
        finally
        {
            IsDeleting = false;
            HideBulkDeleteModal();
        }
    }

    private void ToggleProductSelection(int productId)
    {
        if (SelectedProducts.Contains(productId))
        {
            SelectedProducts.Remove(productId);
        }
        else
        {
            SelectedProducts.Add(productId);
        }
        StateHasChanged();
    }

    private void ToggleSelectAll()
    {
        if (SelectedProducts.Count == _products.Count)
        {
            SelectedProducts.Clear();
        }
        else
        {
            SelectedProducts = _products.Select(p => p.Id).ToList();
        }
        StateHasChanged();
    }

    private async Task ExportSelectedToCsv()
    {
        try
        {
            var productsToExport = _products.Where(p => SelectedProducts.Contains(p.Id)).ToList();
            if (!productsToExport.Any())
            {
                await ShowToast("No Selection", "Please select products to export.", "warning");
                return;
            }

            var csvContent = GenerateCsvContent(productsToExport);
            var fileName = $"products_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            await JSRuntime.InvokeVoidAsync("downloadFile", fileName, "text/csv", csvContent);
            await ShowToast("Export Complete", $"Exported {productsToExport.Count} products to CSV.", "success");
        }
        catch (Exception ex)
        {
            await ShowToast("Export Failed", ex.Message, "danger");
        }
    }

    private string GenerateCsvContent(List<ProductDto> products)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Name,SKU,Category,Price,Stock,Status,Created Date");

        foreach (var product in products)
        {
            sb.AppendLine($"\"{product.Name}\",\"{product.SKU}\",\"{product.CategoryName}\",\"{product.Price}\",\"{product.Stock}\",\"{product.AvailabilityStatus}\",\"{product.CreatedDate:yyyy-MM-dd}\"");
        }

        return sb.ToString();
    }

    private async Task ExportToExcel()
    {
        try
        {
            var productsToExport = _products.Where(p => SelectedProducts.Contains(p.Id)).ToList();
            if (!productsToExport.Any())
            {
                await ShowToast("No Selection", "Please select products to export.", "warning");
                return;
            }

            // In a real implementation, you would generate Excel file
            // For now, we'll export as CSV with .xlsx extension
            var csvContent = GenerateCsvContent(productsToExport);
            var fileName = $"products_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            await JSRuntime.InvokeVoidAsync("downloadFile", fileName, "application/vnd.ms-excel", csvContent);
            await ShowToast("Export Complete", $"Exported {productsToExport.Count} products to Excel.", "success");
        }
        catch (Exception ex)
        {
            await ShowToast("Export Failed", ex.Message, "danger");
        }
    }

    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;

        CurrentPage = page;
        _ = LoadProductsAsync();
    }

    private async Task ChangePageSize(int newPageSize)
    {
        PageSize = newPageSize;
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    private string GetRowClass(ProductDto product)
    {
        var classes = new List<string>();

        if (SelectedProducts.Contains(product.Id))
            classes.Add("table-active");

        if (product.Stock == 0)
            classes.Add("table-warning");

        return string.Join(" ", classes);
    }

    private string GetCardClass(ProductDto product)
    {
        var classes = new List<string>();

        if (SelectedProducts.Contains(product.Id))
            classes.Add("border-primary");

        if (product.Stock == 0)
            classes.Add("border-warning");

        return string.Join(" ", classes);
    }

    private string GetStockClass(ProductDto product)
    {
        if (product.Stock == 0)
            return "badge bg-danger";
        else if (product.Stock <= 10)
            return "badge bg-warning text-dark";
        else
            return "badge bg-success";
    }

    private string GetStatusBadgeClass(ProductDto product)
    {
        return product.AvailabilityStatus switch
        {
            "Active" => "bg-success",
            "Inactive" => "bg-secondary",
            _ => "bg-light text-dark"
        };
    }

    private async Task ShowToast(string title, string message, string type)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("showToast", title, message, type);
        }
        catch (Exception ex)
        {
            // Fallback if toast service isn't available
            Console.WriteLine($"Toast: {title} - {message}");
        }
    }

    private void HandleSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            _searchTimer?.Dispose();
            _ = LoadProductsAsync();
        }
        else if (e.Key == "Escape")
        {
            ClearSearch();
        }
    }

    private string ParseSortBy(string sortBy)
    {
        return sortBy switch
        {
            "name-asc" => "Name",
            "name-desc" => "Name desc",
            "price-asc" => "Price",
            "price-desc" => "Price desc",
            "created-desc" => "CreatedDate desc",
            "created-asc" => "CreatedDate",
            "stock-desc" => "Stock desc",
            _ => "Name"
        };
    }

    // Virtual Scrolling Support
    private async Task LoadMoreProductsAsync()
    {
        if (IsLoading || _products.Count >= TotalCount) return;

        IsLoading = true;
        StateHasChanged();

        try
        {
            var result = await ProductService.GetProductsAsync(
                categoryId: SelectedCategoryId,
                searchTerm: SearchTerm,
                pageNumber: CurrentPage + 1,
                pageSize: PageSize,
                sortBy: ParseSortBy(SortBy));

            _products.AddRange(result.Items);
            CurrentPage++;
        }
        catch (Exception ex)
        {
            await ShowToast("Error loading more products", ex.Message, "danger");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    // Keyboard Shortcuts Handler
    [JSInvokable]
    public async Task HandleKeyboardShortcut(string shortcut)
    {
        switch (shortcut)
        {
            case "delete":
                if (SelectedProducts.Any())
                {
                    ShowBulkDeleteConfirmation();
                }
                else
                {
                    await ShowToast("No Selection", "Please select products to delete.", "info");
                }
                break;
            case "new":
                NavigateToCreate();
                break;
            case "select-all":
                ToggleSelectAll();
                break;
            case "clear-search":
                ClearSearch();
                break;
            case "clear-filters":
                ClearAllFilters();
                break;
            case "export-csv":
                await ExportSelectedToCsv();
                break;
            case "export-excel":
                await ExportToExcel();
                break;
        }
    }

    public void Dispose()
    {
        _searchTimer?.Dispose();
    }
}
