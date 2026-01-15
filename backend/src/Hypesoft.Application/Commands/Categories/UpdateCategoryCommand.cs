using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Categories;

public record UpdateCategoryCommand(string Id, UpdateCategoryDto Category) : IRequest<CategoryDto?>;