using System.Data;

namespace Application.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}