using Hypesoft.Application.Commands.Categories;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        // Verificar se a categoria tem produtos
        var hasProducts = await _categoryRepository.HasProductsAsync(request.Id);
        if (hasProducts)
            throw new InvalidOperationException("Cannot delete category with associated products");

        return await _categoryRepository.DeleteAsync(request.Id);
    }
}