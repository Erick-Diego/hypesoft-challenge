using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products;

public record GetProductsPagedQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResultDto<ProductDto>>;