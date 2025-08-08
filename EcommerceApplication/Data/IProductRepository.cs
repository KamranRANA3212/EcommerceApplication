using EcommerceApplication.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Application.Data
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(string search = null, int? categoryId = null);
        Task<Product> GetByIdAsync(int id);
        Task<int> AddAsync(Product product);
        Task<int> UpdateAsync(Product product);
        Task<int> DeleteAsync(int id);
    }
}