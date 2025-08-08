using Dapper;
using EcommerceApplication.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_Application.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly DapperContext _context;
        public ProductRepository()
        {
            _context = new DapperContext();
        }

        public async Task<int> AddAsync(Product product)
        {
            var sql = @"INSERT INTO Product (ProductName, SKU, Price, CategoryId, Status, Photo)
                         VALUES (@ProductName, @SKU, @Price, @CategoryId, @Status, @Photo);
                         SELECT CAST(SCOPE_IDENTITY() as int);";
            using (var connection = _context.CreateConnection())
            {
                var id = await connection.QuerySingleAsync<int>(sql, product);
                return id;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            var sql = "DELETE FROM Product WHERE Id = @Id";
            using (var connection = _context.CreateConnection())
            {
                return await connection.ExecuteAsync(sql, new { Id = id });
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync(string search = null, int? categoryId = null)
        {
            var sql = "SELECT * FROM Product WHERE 1=1";
            if (!string.IsNullOrEmpty(search))
            {
                sql += " AND (ProductName LIKE @Search OR SKU LIKE @Search)";
            }
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                sql += " AND CategoryId = @CategoryId";
            }
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<Product>(sql, new { Search = "%" + search + "%", CategoryId = categoryId });
            }
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM Product WHERE Id = @Id";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
            }
        }

        public async Task<int> UpdateAsync(Product product)
        {
            var sql = @"UPDATE Product SET
                            ProductName = @ProductName,
                            SKU = @SKU,
                            Price = @Price,
                            CategoryId = @CategoryId,
                            Status = @Status,
                            Photo = @Photo
                        WHERE Id = @Id";
            using (var connection = _context.CreateConnection())
            {
                return await connection.ExecuteAsync(sql, product);
            }
        }
    }
}