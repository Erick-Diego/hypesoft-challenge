using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products;

public record GetProductByIdQuery(string Id) : IRequest<ProductDto?>;