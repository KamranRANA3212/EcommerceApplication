using Application.Repositories;
using Domain.Entities;

namespace Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public Task<IEnumerable<Category>> GetAllAsync() => _categoryRepository.GetAllAsync();
    public Task<Category?> GetAsync(int id) => _categoryRepository.GetByIdAsync(id);
}