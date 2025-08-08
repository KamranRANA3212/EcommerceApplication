using System.Data;
using Microsoft.Data.Sqlite;
using Application.Abstractions;

namespace Infrastructure.Data;

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string dbFilePath)
    {
        var dataSource = dbFilePath.Replace("\\", "/");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dataSource,
            ForeignKeys = true
        }.ToString();
    }

    public IDbConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        return conn;
    }
}