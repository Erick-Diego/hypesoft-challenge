using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Services;
using MongoDB.Driver;

namespace Hypesoft.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _collection;

    public ProductRepository(MongoDbService mongoDbService)
    {
        _collection = mongoDbService.GetCollection<Product>("products");
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id && p.IsActive).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _collection.Find(p => p.IsActive).ToListAsync();
    }

    public async Task<Product> AddAsync(Product entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<Product> UpdateAsync(Product entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(p => p.Id == entity.Id, entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var product = await GetByIdAsync(id);
        if (product == null) return false;

        product.Deactivate();
        await UpdateAsync(product);
        return true;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _collection.Find(p => p.Id == id && p.IsActive).AnyAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(string categoryId)
    {
        return await _collection
            .Find(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(name, "i")),
            Builders<Product>.Filter.Eq(p => p.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _collection
            .Find(p => p.StockQuantity < threshold && p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetPagedAsync(int page, int pageSize)
    {
        return await _collection
            .Find(p => p.IsActive)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return (int)await _collection.CountDocumentsAsync(p => p.IsActive);
    }

    public async Task<decimal> GetTotalStockValueAsync()
    {
        var products = await GetAllAsync();
        return products.Sum(p => p.TotalStockValue);
    }
}