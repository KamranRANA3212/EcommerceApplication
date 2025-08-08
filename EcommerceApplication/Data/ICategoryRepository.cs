using EcommerceApplication.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Application.Data
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
    }
}