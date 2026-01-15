using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products;

public record GetProductsByCategoryQuery(string CategoryId) : IRequest<IEnumerable<ProductDto>>;