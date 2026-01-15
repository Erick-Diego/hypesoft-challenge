using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products;

public record SearchProductsQuery(string SearchTerm) : IRequest<IEnumerable<ProductDto>>;