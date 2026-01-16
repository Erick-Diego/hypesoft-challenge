using AutoMapper;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CreateCategoryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(request.Category.Name);
        if (existingCategory != null)
            throw new InvalidOperationException($"Category with name '{request.Category.Name}' already exists");

        var category = Category.Create(request.Category.Name, request.Category.Description);

        var createdCategory = await _categoryRepository.AddAsync(category);
        return _mapper.Map<CategoryDto>(createdCategory);
    }
}
