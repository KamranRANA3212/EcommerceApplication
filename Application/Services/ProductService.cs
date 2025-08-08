using Application.Repositories;
using Domain.Entities;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public Task<IEnumerable<(Product product, string categoryName)>> SearchAsync(string? search, int? categoryId)
        => _productRepository.GetAllAsync(search, categoryId);

    public Task<Product?> GetAsync(int id) => _productRepository.GetByIdAsync(id);

    public async Task<(bool ok, string? error, int? id)> CreateAsync(Product product)
    {
        var validationError = await ValidateAsync(product, isUpdate: false);
        if (validationError != null) return (false, validationError, null);
        var id = await _productRepository.CreateAsync(product);
        return (true, null, id);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(Product product)
    {
        var validationError = await ValidateAsync(product, isUpdate: true);
        if (validationError != null) return (false, validationError);
        await _productRepository.UpdateAsync(product);
        return (true, null);
    }

    public Task DeleteAsync(int id) => _productRepository.DeleteAsync(id);

    private async Task<string?> ValidateAsync(Product product, bool isUpdate)
    {
        if (string.IsNullOrWhiteSpace(product.ProductName)) return "Product Name is required.";
        if (string.IsNullOrWhiteSpace(product.Sku)) return "SKU is required.";
        if (product.Price <= 0) return "Price must be greater than zero.";
        if (product.CategoryId <= 0) return "Category is required.";
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        if (category == null) return "Selected category does not exist.";

        if (!isUpdate)
        {
            if (await _productRepository.ExistsBySkuAsync(product.Sku)) return "SKU must be unique.";
        }
        else
        {
            if (await _productRepository.ExistsBySkuExcludingIdAsync(product.Sku, product.Id)) return "SKU must be unique.";
        }

        if (!Enum.IsDefined(typeof(ProductStatus), product.Status)) return "Invalid status.";
        return null;
    }
}