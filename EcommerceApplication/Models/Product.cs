using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Application.Models
{
    public enum ProductStatus
    {
        Active = 1,
        Inactive = 0
    }

    public class Product
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [StringLength(100)]
        public string SKU { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        public ProductStatus Status { get; set; }

        public string Photo { get; set; }
    }
}