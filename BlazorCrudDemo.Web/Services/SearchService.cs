using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorCrudDemo.Web.Services
{
    public interface ISearchService<T> where T : class
    {
        Task<SearchResult<T>> SearchAsync(SearchParameters parameters);
        Task<IEnumerable<string>> GetSearchSuggestionsAsync(string query);
        Task<IEnumerable<string>> GetPopularSearchesAsync(int count = 5);
        Task SaveSearchPreferenceAsync(string userId, SearchParameters parameters);
        Task<SearchParameters> GetSavedSearchPreferenceAsync(string userId);
    }

    public class SearchService<T> : ISearchService<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<string, Expression<Func<T, string>>> _searchableProperties;
        private readonly int _maxSuggestions = 10;

        public SearchService(ApplicationDbContext context, 
            Dictionary<string, Expression<Func<T, string>>> searchableProperties)
        {
            _context = context;
            _searchableProperties = searchableProperties;
        }

        public async Task<SearchResult<T>> SearchAsync(SearchParameters parameters)
        {
            var query = _context.Set<T>().AsQueryable();

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = ApplySearchTerm(query, parameters.SearchTerm);
            }

            // Apply filters
            if (parameters.Filters != null)
            {
                query = ApplyFilters(query, parameters.Filters);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = ApplySorting(query, parameters.SortBy, parameters.SortDescending);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            if (parameters.PageSize > 0)
            {
                query = query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize);
            }

            var results = await query.ToListAsync();
            
            return new SearchResult<T>
            {
                Items = results,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize)
            };
        }

        private IQueryable<T> ApplySearchTerm(IQueryable<T> query, string searchTerm)
        {
            // Create a parameter expression for the entity type
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combinedExpression = null;

            // Build OR conditions for each searchable property
            foreach (var property in _searchableProperties)
            {
                var propertyAccess = property.Value.Body;
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var containsCall = Expression.Call(propertyAccess, containsMethod!, Expression.Constant(searchTerm));
                
                combinedExpression = combinedExpression == null 
                    ? containsCall 
                    : Expression.OrElse(combinedExpression, containsCall);
            }

            if (combinedExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        private IQueryable<T> ApplyFilters(IQueryable<T> query, Dictionary<string, object> filters)
        {
            // This is a simplified example. In a real application, you would need to handle
            // different types of filters (range, exact match, contains, etc.)
            foreach (var filter in filters)
            {
                // Implementation depends on your specific filter types and requirements
                // This is a placeholder for the actual filter application logic
            }

            return query;
        }

        private IQueryable<T> ApplySorting(IQueryable<T> query, string sortBy, bool descending)
        {
            // Implementation of dynamic sorting
            // This is a simplified example
            if (descending)
            {
                return query.OrderByDescending(x => EF.Property<object>(x, sortBy));
            }
            return query.OrderBy(x => EF.Property<object>(x, sortBy));
        }

        public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Enumerable.Empty<string>();

            // This is a simplified example. In a real application, you would want to:
            // 1. Check a search suggestions table/cache
            // 2. Fall back to generating suggestions from the data
            // 3. Consider using a dedicated search service like Elasticsearch or Azure Search

            var suggestions = new List<string>();

            // Add some example suggestions
            if (query.StartsWith("pro", StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add("product");
                suggestions.Add("products");
                suggestions.Add("product category");
            }

            // Ensure we don't return more than the max suggestions
            await Task.CompletedTask;
            return suggestions.Take(_maxSuggestions);
        }

        public async Task<IEnumerable<string>> GetPopularSearchesAsync(int count = 5)
        {
            // In a real application, this would come from a search log or analytics
            await Task.CompletedTask;
            return new List<string>
            {
                "laptop",
                "smartphone",
                "headphones",
                "monitor",
                "keyboard"
            }.Take(count);
        }

        public Task SaveSearchPreferenceAsync(string userId, SearchParameters parameters)
        {
            // Implementation would save to a database or user preferences store
            return Task.CompletedTask;
        }

        public Task<SearchParameters> GetSavedSearchPreferenceAsync(string userId)
        {
            // Implementation would retrieve from a database or user preferences store
            return Task.FromResult(new SearchParameters());
        }
    }

    public class SearchParameters
    {
        public string? SearchTerm { get; set; }
        public string? Keywords { get; set; }
        public Dictionary<string, object>? Filters { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStockOnly { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SearchResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
