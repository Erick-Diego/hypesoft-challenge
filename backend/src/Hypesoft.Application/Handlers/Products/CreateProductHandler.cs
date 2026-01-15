using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DTOs;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CreateProductHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validar se a categoria existe
        var categoryExists = await _categoryRepository.ExistsAsync(request.Product.CategoryId);
        if (!categoryExists)
            throw new ArgumentException($"Category with ID {request.Product.CategoryId} not found");

        // Criar produto usando Factory Method do domínio
        var product = Product.Create(
            request.Product.Name,
            request.Product.Description,
            request.Product.Price,
            request.Product.CategoryId,
            request.Product.StockQuantity,
            request.Product.ImageUrl
        );

        // Salvar
        var createdProduct = await _productRepository.AddAsync(product);

        // Retornar DTO
        return _mapper.Map<ProductDto>(createdProduct);
    }
}