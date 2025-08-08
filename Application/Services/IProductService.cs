using Domain.Entities;

namespace Application.Services;

public interface IProductService
{
    Task<IEnumerable<(Product product, string categoryName)>> SearchAsync(string? search, int? categoryId);
    Task<Product?> GetAsync(int id);
    Task<(bool ok, string? error, int? id)> CreateAsync(Product product);
    Task<(bool ok, string? error)> UpdateAsync(Product product);
    Task DeleteAsync(int id);
}