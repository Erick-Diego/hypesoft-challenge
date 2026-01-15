namespace Hypesoft.Application.DTOs;

public record CategoryDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record UpdateCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}