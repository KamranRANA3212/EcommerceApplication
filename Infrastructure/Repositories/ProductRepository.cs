using Application.Abstractions;
using Application.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<(Product product, string categoryName)>> GetAllAsync(string? search, int? categoryId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"SELECT p.Id, p.ProductName, p.SKU, p.Price, p.CategoryId, p.Status, p.Photo, c.Name AS CategoryName
                    FROM dbo.Product p
                    JOIN dbo.Category c ON p.CategoryId = c.Id
                    WHERE (@Search IS NULL OR p.ProductName LIKE '%' + @Search + '%' OR p.SKU LIKE '%' + @Search + '%')
                      AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
                    ORDER BY p.Id DESC";
        var rows = await conn.QueryAsync(sql, new { Search = search, CategoryId = categoryId });
        var result = new List<(Product, string)>();
        foreach (var row in rows)
        {
            var product = new Product
            {
                Id = row.Id,
                ProductName = row.ProductName,
                Sku = row.SKU,
                Price = row.Price,
                CategoryId = row.CategoryId,
                Status = Enum.TryParse<ProductStatus>((string)row.Status, out var st) ? st : ProductStatus.Active,
                Photo = row.Photo as string
            };
            result.Add((product, (string)row.CategoryName));
        }
        return result;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"SELECT TOP 1 Id, ProductName, SKU, Price, CategoryId, Status, Photo FROM dbo.Product WHERE Id = @Id";
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        if (row == null) return null;
        return new Product
        {
            Id = row.Id,
            ProductName = row.ProductName,
            Sku = row.SKU,
            Price = row.Price,
            CategoryId = row.CategoryId,
            Status = Enum.TryParse<ProductStatus>((string)row.Status, out var st) ? st : ProductStatus.Active,
            Photo = row.Photo as string
        };
    }

    public async Task<bool> ExistsBySkuAsync(string sku)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = "SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.Product WHERE SKU = @Sku) THEN 1 ELSE 0 END AS BIT)";
        return await conn.ExecuteScalarAsync<bool>(sql, new { Sku = sku });
    }

    public async Task<bool> ExistsBySkuExcludingIdAsync(string sku, int excludeId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = "SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.Product WHERE SKU = @Sku AND Id <> @Id) THEN 1 ELSE 0 END AS BIT)";
        return await conn.ExecuteScalarAsync<bool>(sql, new { Sku = sku, Id = excludeId });
    }

    public async Task<int> CreateAsync(Product product)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO dbo.Product (ProductName, SKU, Price, CategoryId, Status, Photo)
                    VALUES (@ProductName, @SKU, @Price, @CategoryId, @Status, @Photo);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
        var id = await conn.ExecuteScalarAsync<int>(sql, new
        {
            ProductName = product.ProductName,
            SKU = product.Sku,
            Price = product.Price,
            CategoryId = product.CategoryId,
            Status = product.Status.ToString(),
            Photo = product.Photo
        });
        return id;
    }

    public async Task UpdateAsync(Product product)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"UPDATE dbo.Product
                    SET ProductName = @ProductName,
                        SKU = @SKU,
                        Price = @Price,
                        CategoryId = @CategoryId,
                        Status = @Status,
                        Photo = @Photo
                    WHERE Id = @Id";
        await conn.ExecuteAsync(sql, new
        {
            Id = product.Id,
            ProductName = product.ProductName,
            SKU = product.Sku,
            Price = product.Price,
            CategoryId = product.CategoryId,
            Status = product.Status.ToString(),
            Photo = product.Photo
        });
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = "DELETE FROM dbo.Product WHERE Id = @Id";
        await conn.ExecuteAsync(sql, new { Id = id });
    }
}