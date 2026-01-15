namespace Hypesoft.Application.DTOs;

public record ProductDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public string? ImageUrl { get; init; }
    public string? Sku { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    public decimal TotalStockValue { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateProductDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public string? ImageUrl { get; init; }
}

public record UpdateProductDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string CategoryId { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}

public record UpdateStockDto
{
    public int Quantity { get; init; }
}