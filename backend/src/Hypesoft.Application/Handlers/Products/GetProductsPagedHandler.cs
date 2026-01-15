using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

public class GetProductsPagedHandler : IRequestHandler<GetProductsPagedQuery, PaginatedResultDto<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsPagedHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<ProductDto>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetPagedAsync(request.Page, request.PageSize);
        var totalCount = await _productRepository.GetTotalCountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PaginatedResultDto<ProductDto>
        {
            Items = _mapper.Map<IEnumerable<ProductDto>>(products),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPrevious = request.Page > 1,
            HasNext = request.Page < totalPages
        };
    }
}