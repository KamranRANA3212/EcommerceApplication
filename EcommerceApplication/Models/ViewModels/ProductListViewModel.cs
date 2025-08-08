using EcommerceApplication.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EcommerceApplication.Models.ViewModels
{
    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public List<Category> Categories { get; set; } = new List<Category>();
        
        // Search and filter properties
        public string SearchTerm { get; set; }
        public int? CategoryFilter { get; set; }
        
        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
} 