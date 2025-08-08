using System.ComponentModel.DataAnnotations;

namespace Ecommerce_Application.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }
    }
}