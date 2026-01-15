using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hypesoft.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os produtos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        _logger.LogInformation("Getting all products");
        var products = await _mediator.Send(new GetAllProductsQuery());
        return Ok(products);
    }

    /// <summary>
    /// Obtém produtos paginados
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PaginatedResultDto<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResultDto<ProductDto>>> GetPaged(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting products paged - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        var result = await _mediator.Send(new GetProductsPagedQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Obtém um produto por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(string id)
    {
        _logger.LogInformation("Getting product by ID: {ProductId}", id);
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Busca produtos por nome
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string term)
    {
        _logger.LogInformation("Searching products with term: {SearchTerm}", term);
        var products = await _mediator.Send(new SearchProductsQuery(term));
        return Ok(products);
    }

    /// <summary>
    /// Obtém produtos por categoria
    /// </summary>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(string categoryId)
    {
        _logger.LogInformation("Getting products by category: {CategoryId}", categoryId);
        var products = await _mediator.Send(new GetProductsByCategoryQuery(categoryId));
        return Ok(products);
    }

    /// <summary>
    /// Obtém produtos com estoque baixo
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStock([FromQuery] int threshold = 10)
    {
        _logger.LogInformation("Getting low stock products with threshold: {Threshold}", threshold);
        var products = await _mediator.Send(new GetLowStockProductsQuery(threshold));
        return Ok(products);
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", dto.Name);
        
        try
        {
            var product = await _mediator.Send(new CreateProductCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating product");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Update(string id, [FromBody] UpdateProductDto dto)
    {
        _logger.LogInformation("Updating product: {ProductId}", id);
        
        try
        {
            var product = await _mediator.Send(new UpdateProductCommand(id, dto));
            
            if (product == null)
            {
                _logger.LogWarning("Product not found for update: {ProductId}", id);
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating product");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza o estoque de um produto
    /// </summary>
    [HttpPatch("{id}/stock")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> UpdateStock(string id, [FromBody] UpdateStockDto dto)
    {
        _logger.LogInformation("Updating stock for product: {ProductId} to {Quantity}", id, dto.Quantity);
        
        try
        {
            var product = await _mediator.Send(new UpdateStockCommand(id, dto.Quantity));
            
            if (product == null)
            {
                _logger.LogWarning("Product not found for stock update: {ProductId}", id);
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating stock");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deleta um produto (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting product: {ProductId}", id);
        var deleted = await _mediator.Send(new DeleteProductCommand(id));
        
        if (!deleted)
        {
            _logger.LogWarning("Product not found for deletion: {ProductId}", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        return NoContent();
    }
}