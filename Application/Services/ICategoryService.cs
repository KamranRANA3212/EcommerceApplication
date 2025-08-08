using Domain.Entities;

namespace Application.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetAsync(int id);
}