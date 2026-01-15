using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hypesoft.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as categorias
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        _logger.LogInformation("Getting all categories");
        var categories = await _mediator.Send(new GetAllCategoriesQuery());
        return Ok(categories);
    }

    /// <summary>
    /// Obtém uma categoria por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(string id)
    {
        _logger.LogInformation("Getting category by ID: {CategoryId}", id);
        var category = await _mediator.Send(new GetCategoryByIdQuery(id));
        
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        return Ok(category);
    }

    /// <summary>
    /// Cria uma nova categoria
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        _logger.LogInformation("Creating new category: {CategoryName}", dto.Name);
        
        try
        {
            var category = await _mediator.Send(new CreateCategoryCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating category");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza uma categoria existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> Update(string id, [FromBody] UpdateCategoryDto dto)
    {
        _logger.LogInformation("Updating category: {CategoryId}", id);
        
        try
        {
            var category = await _mediator.Send(new UpdateCategoryCommand(id, dto));
            
            if (category == null)
            {
                _logger.LogWarning("Category not found for update: {CategoryId}", id);
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating category");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deleta uma categoria (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting category: {CategoryId}", id);
        
        try
        {
            var deleted = await _mediator.Send(new DeleteCategoryCommand(id));
            
            if (!deleted)
            {
                _logger.LogWarning("Category not found for deletion: {CategoryId}", id);
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete category with products");
            return BadRequest(new { message = ex.Message });
        }
    }
}