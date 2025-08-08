using Application.Abstractions;
using Microsoft.Data.Sqlite;

namespace Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = _connectionFactory.CreateConnection() as SqliteConnection;
        if (connection == null) return;
        await connection.OpenAsync();

        var createCategory = @"CREATE TABLE IF NOT EXISTS Category (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL UNIQUE
        );";

        var createProduct = @"CREATE TABLE IF NOT EXISTS Product (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProductName TEXT NOT NULL,
            SKU TEXT NOT NULL UNIQUE,
            Price REAL NOT NULL CHECK(Price > 0),
            CategoryId INTEGER NOT NULL,
            Status TEXT NOT NULL CHECK(Status IN ('Active','Inactive')),
            Photo TEXT NULL,
            FOREIGN KEY (CategoryId) REFERENCES Category(Id)
        );";

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = createCategory;
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = createProduct;
            await cmd.ExecuteNonQueryAsync();
        }

        var seedCategories = @"INSERT OR IGNORE INTO Category (Id, Name) VALUES
            (1, 'Electronics'),
            (2, 'Home Appliances'),
            (3, 'Books'),
            (4, 'Furniture');";
        using (var seedCmd = connection.CreateCommand())
        {
            seedCmd.CommandText = seedCategories;
            await seedCmd.ExecuteNonQueryAsync();
        }

        await connection.CloseAsync();
    }
}