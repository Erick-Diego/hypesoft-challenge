namespace Hypesoft.Application.DTOs;

public record DashboardDto
{
    public int TotalProducts { get; init; }
    public decimal TotalStockValue { get; init; }
    public int LowStockProductsCount { get; init; }
    public IEnumerable<ProductDto> LowStockProducts { get; init; } = Array.Empty<ProductDto>();
    public IEnumerable<CategoryStatsDto> CategoryStats { get; init; } = Array.Empty<CategoryStatsDto>();
}

public record CategoryStatsDto
{
    public string CategoryId { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int ProductCount { get; init; }
}