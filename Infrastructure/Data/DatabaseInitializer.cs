using Application.Abstractions;
using System.Data;

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
        using var connection = _connectionFactory.CreateConnection();
        await (connection as dynamic).OpenAsync();

        var createCategory = @"IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Category](
        [Id] INT NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL UNIQUE
    );
END";

        var createProduct = @"IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Product](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ProductName] NVARCHAR(200) NOT NULL,
        [SKU] NVARCHAR(100) NOT NULL UNIQUE,
        [Price] DECIMAL(18,2) NOT NULL CHECK ([Price] > 0),
        [CategoryId] INT NOT NULL,
        [Status] NVARCHAR(20) NOT NULL CHECK ([Status] IN ('Active','Inactive')),
        [Photo] NVARCHAR(500) NULL,
        CONSTRAINT FK_Product_Category FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category]([Id])
    );
END";

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = createCategory;
            await (cmd as dynamic).ExecuteNonQueryAsync();
            cmd.CommandText = createProduct;
            await (cmd as dynamic).ExecuteNonQueryAsync();
        }

        var seedCategories = @"MERGE [dbo].[Category] AS target
USING (VALUES (1,'Electronics'),(2,'Home Appliances'),(3,'Books'),(4,'Furniture')) AS src([Id],[Name])
ON target.[Id] = src.[Id]
WHEN NOT MATCHED THEN INSERT([Id],[Name]) VALUES(src.[Id],src.[Name]);";

        using (var cmd2 = connection.CreateCommand())
        {
            cmd2.CommandText = seedCategories;
            await (cmd2 as dynamic).ExecuteNonQueryAsync();
        }

        await (connection as dynamic).CloseAsync();
    }
}