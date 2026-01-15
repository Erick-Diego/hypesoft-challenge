using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Products;

public record CreateProductCommand(CreateProductDto Product) : IRequest<ProductDto>;