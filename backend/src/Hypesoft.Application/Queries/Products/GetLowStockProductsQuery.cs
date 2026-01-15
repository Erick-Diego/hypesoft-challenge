using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products;

public record GetLowStockProductsQuery(int Threshold = 10) : IRequest<IEnumerable<ProductDto>>;