using EcommerceApplication.Domain.Entities;
using EcommerceApplication.Models.ViewModels;
using EcommerceApplication.Services.Interfaces;
using System.Data;
using Dapper;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Web.Hosting;

namespace EcommerceApplication.Services
{
    public class ProductService : IProductService
    {
        private readonly IDbConnection _db;

        public ProductService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<ProductListViewModel> GetProductsAsync(string searchTerm, int? categoryFilter, int page = 1, int pageSize = 10)
        {
            var sql = @"SELECT p.*, c.Name as CategoryName 
                        FROM Product p 
                        INNER JOIN Category c ON p.CategoryId = c.Id
                        WHERE (@searchTerm IS NULL OR p.ProductName LIKE '%' + @searchTerm + '%' OR p.SKU LIKE '%' + @searchTerm + '%')
                        AND (@categoryFilter IS NULL OR p.CategoryId = @categoryFilter)
                        ORDER BY p.Id DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                        SELECT COUNT(*) FROM Product p WHERE (@searchTerm IS NULL OR p.ProductName LIKE '%' + @searchTerm + '%' OR p.SKU LIKE '%' + @searchTerm + '%') AND (@categoryFilter IS NULL OR p.CategoryId = @categoryFilter);";
            var offset = (page - 1) * pageSize;
            using (var multi = await _db.QueryMultipleAsync(sql, new { searchTerm, categoryFilter, Offset = offset, PageSize = pageSize }))
            {
                var products = (await multi.ReadAsync<ProductViewModel>()).ToList();
                var totalItems = await multi.ReadFirstAsync<int>();
                var categories = await GetAllCategoriesAsync();
                return new ProductListViewModel
                {
                    Products = products,
                    Categories = categories,
                    SearchTerm = searchTerm,
                    CategoryFilter = categoryFilter,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
            }
        }

        public async Task<ProductViewModel> GetProductByIdAsync(int id)
        {
            var sql = @"SELECT p.*, c.Name as CategoryName FROM Product p INNER JOIN Category c ON p.CategoryId = c.Id WHERE p.Id = @id";
            var product = await _db.QueryFirstOrDefaultAsync<ProductViewModel>(sql, new { id });
            if (product == null) return null;
            product.Categories = await GetAllCategoriesAsync();
            return product;
        }

        public async Task<bool> CreateProductAsync(ProductViewModel productViewModel)
        {
            try
            {
                var sql = @"INSERT INTO Product (ProductName, SKU, Price, CategoryId, Status, Photo) VALUES (@ProductName, @SKU, @Price, @CategoryId, @Status, @Photo); SELECT CAST(SCOPE_IDENTITY() as int);";
                var product = new Product
                {
                    ProductName = productViewModel.ProductName,
                    SKU = productViewModel.SKU,
                    Price = productViewModel.Price,
                    CategoryId = productViewModel.CategoryId,
                    Status = productViewModel.Status
                };
                if (productViewModel.PhotoFile != null)
                {
                    product.Photo = await SavePhotoAsync(productViewModel.PhotoFile);
                }
                var id = await _db.ExecuteScalarAsync<int>(sql, product);
                return id > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(ProductViewModel productViewModel)
        {
            try
            {
                var sql = @"UPDATE Product SET ProductName = @ProductName, SKU = @SKU, Price = @Price, CategoryId = @CategoryId, Status = @Status, Photo = @Photo WHERE Id = @Id";
                var product = await _db.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Product WHERE Id = @Id", new { productViewModel.Id });
                if (product == null) return false;
                product.ProductName = productViewModel.ProductName;
                product.SKU = productViewModel.SKU;
                product.Price = productViewModel.Price;
                product.CategoryId = productViewModel.CategoryId;
                product.Status = productViewModel.Status;
                if (productViewModel.PhotoFile != null)
                {
                    if (!string.IsNullOrEmpty(product.Photo))
                    {
                        DeletePhoto(product.Photo);
                    }
                    product.Photo = await SavePhotoAsync(productViewModel.PhotoFile);
                }
                var affected = await _db.ExecuteAsync(sql, product);
                return affected > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _db.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Product WHERE Id = @id", new { id });
                if (product == null) return false;
                if (!string.IsNullOrEmpty(product.Photo))
                {
                    DeletePhoto(product.Photo);
                }
                var sql = "DELETE FROM Product WHERE Id = @id";
                var affected = await _db.ExecuteAsync(sql, new { id });
                return affected > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var sql = "SELECT * FROM Category ORDER BY Name";
            var categories = (await _db.QueryAsync<Category>(sql)).ToList();
            return categories;
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, int excludeId = 0)
        {
            var sql = "SELECT COUNT(1) FROM Product WHERE SKU = @sku AND Id != @excludeId";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { sku, excludeId });
            return count == 0;
        }

        private async Task<string> SavePhotoAsync(HttpPostedFileBase photoFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Only JPG, JPEG, and PNG files are allowed.");
            }
            if (photoFile.ContentLength > 25 * 1024)
            {
                throw new ArgumentException("File size must be less than 25KB.");
            }

            var uploadsFolder = HostingEnvironment.MapPath("~/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            photoFile.SaveAs(filePath);

            return $"uploads/{fileName}";
        }

        private void DeletePhoto(string photoPath)
        {
            if (string.IsNullOrEmpty(photoPath)) return;

            var fullPath = HostingEnvironment.MapPath("~" + (photoPath.StartsWith("/") ? photoPath : "/" + photoPath));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
} 