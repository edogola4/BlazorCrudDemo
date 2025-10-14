using AutoMapper;
using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Web.Mapping;

/// <summary>
/// AutoMapper profile for mapping between models and DTOs.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the MappingProfile class.
    /// </summary>
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dto => dto.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dto => dto.IsInStock, opt => opt.MapFrom(src => src.Stock > 0))
            .ForMember(dto => dto.FormattedPrice, opt => opt.MapFrom(src => src.Price.ToString("C")))
            .ForMember(dto => dto.StockStatus, opt => opt.MapFrom(src => src.Stock > 0 ? $"{src.Stock} in stock" : "Out of stock"))
            .ForMember(dto => dto.AvailabilityStatus, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"))
            .ForMember(dto => dto.TruncatedDescription, opt => opt.MapFrom(src =>
                src.Description != null && src.Description.Length > 100
                    ? src.Description.Substring(0, 100) + "..."
                    : src.Description));

        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dto => dto.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0))
            .ForMember(dto => dto.ActiveProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count(p => p.IsActive) : 0))
            .ForMember(dto => dto.HasProducts, opt => opt.MapFrom(src => src.Products != null && src.Products.Count > 0))
            .ForMember(dto => dto.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"))
            .ForMember(dto => dto.DisplayNameWithCount, opt => opt.MapFrom(src => 
                $"{src.Name} ({(src.Products != null ? src.Products.Count : 0)})"))
            .ForMember(dto => dto.TruncatedDescription, opt => opt.MapFrom(src =>
                src.Description != null && src.Description.Length > 100
                    ? src.Description.Substring(0, 100) + "..."
                    : src.Description))
            .ForMember(dto => dto.ProductSummary, opt => opt.MapFrom(src =>
                src.Products != null && src.Products.Count > 0
                    ? $"{src.Products.Count} product{(src.Products.Count == 1 ? "" : "s")} ({src.Products.Count(p => p.IsActive)} active)"
                    : "No products"));

        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // PaginatedResult mappings
        CreateMap(typeof(PaginatedResult<>), typeof(PaginatedResult<>));

        // Custom type converters for complex scenarios
        CreateMap<string, decimal?>()
            .ConvertUsing((src, dest) =>
                string.IsNullOrEmpty(src) ? null : decimal.TryParse(src, out var result) ? result : null);

        CreateMap<decimal?, string>()
            .ConvertUsing((src, dest) => src.HasValue ? src.Value.ToString("F2") : string.Empty);

        // Date formatting for display
        CreateMap<DateTime, string>()
            .ConvertUsing(src => src.ToString("MMM dd, yyyy HH:mm"));

        CreateMap<DateTime?, string>()
            .ConvertUsing(src => src.HasValue ? src.Value.ToString("MMM dd, yyyy HH:mm") : string.Empty);

        // Boolean to string conversions for display
        CreateMap<bool, string>()
            .ConvertUsing(src => src ? "Yes" : "No");

        CreateMap<bool?, string>()
            .ConvertUsing(src => src == true ? "Yes" : src == false ? "No" : "Unknown");

        // Price formatting for display
        CreateMap<decimal, string>()
            .ConvertUsing(src => src.ToString("C"));

        CreateMap<decimal?, string>()
            .ConvertUsing(src => src.HasValue ? src.Value.ToString("C") : string.Empty);

        // Stock status formatting
        CreateMap<int, string>()
            .ConvertUsing(src => src > 0 ? $"{src} in stock" : "Out of stock");

        // Category with product count formatting
        CreateMap<Category, string>()
            .ConvertUsing(src => $"{src.Name} ({(src.Products != null ? src.Products.Count : 0)})");
    }
}

/// <summary>
/// Extension methods for AutoMapper integration.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps an object to the specified destination type.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>The mapped destination object.</returns>
    public static TDestination MapTo<TDestination>(this object source)
        where TDestination : class
    {
        var mapper = ServiceLocator.GetRequiredService<IMapper>();
        return mapper.Map<TDestination>(source);
    }

    /// <summary>
    /// Maps an object to an existing destination object.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="destination">The destination object.</param>
    /// <returns>The mapped destination object.</returns>
    public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        where TSource : class
        where TDestination : class
    {
        var mapper = ServiceLocator.GetRequiredService<IMapper>();
        return mapper.Map(source, destination);
    }

    /// <summary>
    /// Projects a queryable source to the specified destination type.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <returns>The projected queryable.</returns>
    public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source)
        where TDestination : class
    {
        var mapper = ServiceLocator.GetRequiredService<IMapper>();
        return mapper.ProjectTo<TDestination>(source);
    }
}

/// <summary>
/// Service locator for accessing services in static contexts.
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Sets the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets a required service from the service provider.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance.</returns>
    public static T GetRequiredService<T>()
        where T : class
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("Service provider has not been initialized.");
        }

        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a service from the service provider.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance or null if not found.</returns>
    public static T? GetService<T>()
        where T : class
    {
        return _serviceProvider != null ? _serviceProvider.GetService<T>() : null;
    }
}
