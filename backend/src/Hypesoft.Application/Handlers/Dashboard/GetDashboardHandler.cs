using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Dashboard;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Dashboard;

public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetDashboardHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var totalProducts = await _productRepository.GetTotalCountAsync();

        var totalStockValue = await _productRepository.GetTotalStockValueAsync();

        var lowStockProducts = await _productRepository.GetLowStockProductsAsync();
        var lowStockProductDtos = _mapper.Map<IEnumerable<ProductDto>>(lowStockProducts);

        var categories = await _categoryRepository.GetAllAsync();
        var allProducts = await _productRepository.GetAllAsync();

        var categoryStats = categories.Select(category => new CategoryStatsDto
        {
            CategoryId = category.Id,
            CategoryName = category.Name,
            ProductCount = allProducts.Count(p => p.CategoryId == category.Id)
        }).ToList();

        return new DashboardDto
        {
            TotalProducts = totalProducts,
            TotalStockValue = totalStockValue,
            LowStockProductsCount = lowStockProductDtos.Count(),
            LowStockProducts = lowStockProductDtos,
            CategoryStats = categoryStats
        };
    }
}