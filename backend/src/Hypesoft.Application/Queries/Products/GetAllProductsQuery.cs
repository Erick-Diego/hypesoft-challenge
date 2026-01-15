using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products;

public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>;