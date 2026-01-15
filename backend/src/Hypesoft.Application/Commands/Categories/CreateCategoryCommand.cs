using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Categories;

public record CreateCategoryCommand(CreateCategoryDto Category) : IRequest<CategoryDto>;