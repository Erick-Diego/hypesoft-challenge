namespace Hypesoft.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
    
    // Computed Properties (não salvos no banco)
    public bool IsLowStock => StockQuantity < 10;
    public bool IsOutOfStock => StockQuantity <= 0;
    public decimal TotalStockValue => Price * StockQuantity;
    
    // Factory Method
    public static Product Create(
        string name, 
        string description, 
        decimal price, 
        string categoryId, 
        int stockQuantity,
        string? imageUrl = null,
        string? sku = null)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));
        
        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            StockQuantity = stockQuantity,
            ImageUrl = imageUrl,
            Sku = sku ?? GenerateSku()
        };
    }
    
    // Domain Methods
    public void Update(
        string name, 
        string description, 
        decimal price, 
        string categoryId, 
        string? imageUrl = null)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
        
        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        
        if (quantity > StockQuantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {StockQuantity}");
        
        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private static string GenerateSku()
    {
        return $"SKU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}