using EcommerceApplication.Models.ViewModels;
using EcommerceApplication.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EcommerceApplication.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductListViewModel> GetProductsAsync(string searchTerm, int? categoryFilter, int page = 1, int pageSize = 10);
        Task<ProductViewModel> GetProductByIdAsync(int id);
        Task<bool> CreateProductAsync(ProductViewModel productViewModel);
        Task<bool> UpdateProductAsync(ProductViewModel productViewModel);
        Task<bool> DeleteProductAsync(int id);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<bool> IsSkuUniqueAsync(string sku, int excludeId = 0);
    }
} 