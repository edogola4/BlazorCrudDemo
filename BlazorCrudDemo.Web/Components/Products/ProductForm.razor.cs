using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BlazorCrudDemo.Web.Components.Products;

public partial class ProductForm : ComponentBase, IDisposable
{
    [Parameter] public int? Id { get; set; }
    [Inject] private IProductService ProductService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private ProductDto Product { get; set; } = new();
    private EditContext EditContext { get; set; } = default!;
    private List<CategoryDto> Categories { get; set; } = new();
    private bool IsEdit => Id.HasValue;
    private bool IsSubmitting { get; set; }
    private bool ShowPreview { get; set; } = true;
    private bool ShowSuccess { get; set; }
    private bool IsDirty { get; set; }
    private string? DraftKey => $"product_draft_{Id}";
    private System.Timers.Timer? AutoSaveTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadCategories();
        if (IsEdit)
        {
            await LoadProduct();
        }
        else
        {
            Product = new ProductDto();
            LoadDraft();
        }
        EditContext = new EditContext(Product);
        EditContext.OnFieldChanged += OnFieldChanged;
        EditContext.OnValidationStateChanged += OnValidationStateChanged;

        // Setup auto-save
        AutoSaveTimer = new System.Timers.Timer(30000); // 30 seconds
        AutoSaveTimer.Elapsed += async (sender, args) => await AutoSaveDraft();
        AutoSaveTimer.Start();
    }

    private async Task LoadCategories()
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

    private async Task LoadProduct()
    {
        if (Id.HasValue)
        {
            try
            {
                var product = await ProductService.GetProductAsync(Id.Value);
                Product = product ?? new ProductDto();
            }
            catch (Exception ex)
            {
                await ShowToast("Error loading product", ex.Message, "danger");
            }
        }
    }

    private void LoadDraft()
    {
        try
        {
            var draft = JSRuntime.InvokeAsync<string>("localStorage.getItem", DraftKey);
            if (!string.IsNullOrEmpty(draft.Result))
            {
                Product = JsonSerializer.Deserialize<ProductDto>(draft.Result) ?? new ProductDto();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading draft: {ex.Message}");
        }
    }

    private async Task AutoSaveDraft()
    {
        if (IsDirty)
        {
            try
            {
                var draft = JsonSerializer.Serialize(Product);
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", DraftKey, draft);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error auto-saving draft: {ex.Message}");
            }
        }
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        IsDirty = true;
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        // Handle validation state changes
    }

    private void GenerateSKU()
    {
        Product.SKU = $"SKU-{DateTime.Now:yyyyMMddHHmmss}";
        IsDirty = true;
    }

    private void AdjustStock(int delta)
    {
        Product.Stock = Math.Max(0, Product.Stock + delta);
        IsDirty = true;
    }

    private async Task HandleValidSubmit()
    {
        IsSubmitting = true;
        try
        {
            if (IsEdit)
            {
                var updateDto = new UpdateProductDto
                {
                    Id = Product.Id,
                    Name = Product.Name,
                    SKU = Product.SKU,
                    Description = Product.Description,
                    Price = Product.Price,
                    Stock = Product.Stock,
                    CategoryId = Product.CategoryId,
                    ImageUrl = Product.ImageUrl,
                    IsActive = Product.IsActive
                };
                await ProductService.UpdateProductAsync(updateDto);
            }
            else
            {
                var createDto = new CreateProductDto
                {
                    Name = Product.Name,
                    SKU = Product.SKU,
                    Description = Product.Description,
                    Price = Product.Price,
                    Stock = Product.Stock,
                    CategoryId = Product.CategoryId,
                    ImageUrl = Product.ImageUrl,
                    IsActive = Product.IsActive
                };
                await ProductService.CreateProductAsync(createDto);
            }

            IsDirty = false;
            ClearDraft();
            ShowSuccess = true;
        }
        catch (Exception ex)
        {
            await ShowToast("Error saving product", ex.Message, "danger");
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private async Task ResetForm()
    {
        if (await ConfirmAction("Reset Form", "Are you sure you want to reset all changes?"))
        {
            if (IsEdit)
            {
                await LoadProduct();
            }
            else
            {
                Product = new ProductDto();
                EditContext = new EditContext(Product);
            }
            IsDirty = false;
            ClearDraft();
        }
    }

    private async Task Cancel()
    {
        if (IsDirty)
        {
            if (await ConfirmAction("Unsaved Changes", "You have unsaved changes. Are you sure you want to leave?"))
            {
                Navigation.NavigateTo("/products");
            }
        }
        else
        {
            Navigation.NavigateTo("/products");
        }
    }

    private void ClearDraft()
    {
        try
        {
            JSRuntime.InvokeVoidAsync("localStorage.removeItem", DraftKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing draft: {ex.Message}");
        }
    }

    private async Task<bool> ConfirmAction(string title, string message)
    {
        try
        {
            return await JSRuntime.InvokeAsync<bool>("confirm", $"{title}: {message}");
        }
        catch
        {
            return false;
        }
    }

    private async Task ShowToast(string title, string message, string type)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("showToast", title, message, type);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Toast error: {ex.Message}");
        }
    }

    private void CloseSuccessModal()
    {
        ShowSuccess = false;
        Navigation.NavigateTo("/products");
    }

    public void Dispose()
    {
        AutoSaveTimer?.Dispose();
    }
}
