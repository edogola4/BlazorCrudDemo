using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorCrudDemo.Web.Models
{
    public class SearchBaseModel
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class ProductSearchModel : SearchBaseModel
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public List<int>? CategoryIds { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public bool? IsActive { get; set; }
        public bool InStockOnly { get; set; }
        public string? Keywords { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class SearchFilter
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = "==";
        public object? Value { get; set; }
    }

    public class SearchSuggestion
    {
        public string Text { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    public class SearchFacet
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<FacetValue> Values { get; set; } = new();
    }

    public class FacetValue
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool Selected { get; set; }
    }

    public class SearchResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public List<SearchFacet> Facets { get; set; } = new();
        public List<SearchSuggestion> Suggestions { get; set; } = new();
        public string? DidYouMean { get; set; }
        public TimeSpan SearchDuration { get; set; }
    }
}
