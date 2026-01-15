using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Products;

public record UpdateProductCommand(string Id, UpdateProductDto Product) : IRequest<ProductDto?>;