using Application.Abstractions;
using Application.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = "SELECT Id, Name FROM dbo.Category ORDER BY Name";
        return await conn.QueryAsync<Category>(sql);
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = "SELECT Id, Name FROM dbo.Category WHERE Id = @Id";
        return await conn.QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
    }
}