using Dapper;
using EcommerceApplication.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce_Application.Data
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DapperContext _context;

        public CategoryRepository()
        {
            _context = new DapperContext();
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            var sql = "SELECT Id, Name FROM Category ORDER BY Name";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<Category>(sql);
            }
        }

      /*  Task<IEnumerable<Category>> ICategoryRepository.GetAllAsync()
        {
            throw new System.NotImplementedException();
        }*/

       
    }
}