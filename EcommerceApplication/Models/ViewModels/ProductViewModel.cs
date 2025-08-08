using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EcommerceApplication.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace EcommerceApplication.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(20, ErrorMessage = "SKU cannot exceed 20 characters")]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public ProductStatus Status { get; set; }

        [Display(Name = "Product Photo")]
        public IFormFile PhotoFile { get; set; }

        public string Photo { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime? LastModified { get; set; }

        // Navigation properties
        public string CategoryName { get; set; } = string.Empty;
        public List<Category> Categories { get; set; } = new List<Category>();
    }
} 