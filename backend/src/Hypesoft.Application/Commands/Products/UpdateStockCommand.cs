using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Products;

public record UpdateStockCommand(string Id, int Quantity) : IRequest<ProductDto?>;