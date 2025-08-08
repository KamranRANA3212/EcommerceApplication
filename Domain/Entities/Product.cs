namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public ProductStatus Status { get; set; }
    public string? Photo { get; set; }
}