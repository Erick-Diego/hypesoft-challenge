using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Services;
using MongoDB.Driver;

namespace Hypesoft.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<Category> _collection;
    private readonly IProductRepository _productRepository;

    public CategoryRepository(MongoDbService mongoDbService, IProductRepository productRepository)
    {
        _collection = mongoDbService.GetCollection<Category>("categories");
        _productRepository = productRepository;
    }

    public async Task<Category?> GetByIdAsync(string id)
    {
        return await _collection.Find(c => c.Id == id && c.IsActive).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _collection.Find(c => c.IsActive).ToListAsync();
    }

    public async Task<Category> AddAsync(Category entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<Category> UpdateAsync(Category entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(c => c.Id == entity.Id, entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var category = await GetByIdAsync(id);
        if (category == null) return false;

        // Soft delete
        category.Deactivate();
        await UpdateAsync(category);
        return true;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _collection.Find(c => c.Id == id && c.IsActive).AnyAsync();
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _collection
            .Find(c => c.Name.ToLower() == name.ToLower() && c.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasProductsAsync(string categoryId)
    {
        var products = await _productRepository.GetByCategoryIdAsync(categoryId);
        return products.Any();
    }
}