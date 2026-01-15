using AutoMapper;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto?>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public UpdateCategoryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDto?> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category == null)
            return null;

        // Verificar se outro categoria já tem esse nome
        var existingCategory = await _categoryRepository.GetByNameAsync(request.Category.Name);
        if (existingCategory != null && existingCategory.Id != request.Id)
            throw new InvalidOperationException($"Category with name '{request.Category.Name}' already exists");

        // Atualizar usando método de domínio
        category.Update(request.Category.Name, request.Category.Description);

        var updatedCategory = await _categoryRepository.UpdateAsync(category);
        return _mapper.Map<CategoryDto>(updatedCategory);
    }
}