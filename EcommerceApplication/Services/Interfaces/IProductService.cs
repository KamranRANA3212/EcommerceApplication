using EcommerceApplication.Models.ViewModels;
using EcommerceApplication.Domain.Entities;
using System.Collections.Generic;

namespace EcommerceApplication.Services.Interfaces
{
    public interface IProductService
    {
        ProductListViewModel GetProducts(string searchTerm, int? categoryFilter, int page = 1, int pageSize = 10);
        ProductViewModel GetProductById(int id);
        bool CreateProduct(ProductViewModel productViewModel);
        bool UpdateProduct(ProductViewModel productViewModel);
        bool DeleteProduct(int id);
        List<Category> GetAllCategories();
        bool IsSkuUnique(string sku, int excludeId = 0);
    }
} 