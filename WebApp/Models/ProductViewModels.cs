using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApp.Models;

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Photo { get; set; }
}

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Active";

    [Display(Name = "Product Photo")]
    public IFormFile? PhotoFile { get; set; }

    public string? ExistingPhotoPath { get; set; }
}

public class ProductIndexFilter
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
}