using Domain.Entities;

namespace Application.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<(Product product, string categoryName)>> GetAllAsync(string? search, int? categoryId);
    Task<Product?> GetByIdAsync(int id);
    Task<bool> ExistsBySkuAsync(string sku);
    Task<bool> ExistsBySkuExcludingIdAsync(string sku, int excludeId);
    Task<int> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}