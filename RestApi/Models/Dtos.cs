using System.ComponentModel.DataAnnotations;

namespace RestApi.Models;

// ===== Product DTOs =====
public record ProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool IsActive,
    DateTime CreatedAt,
    int CategoryId,
    string CategoryName
);

public record CreateProductDto(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0.01, 999999.99)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    int CategoryId
);

public record UpdateProductDto(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0.01, 999999.99)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    bool IsActive,
    int CategoryId
);

// ===== Category DTOs =====
public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    int ProductCount
);

public record CreateCategoryDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description
);
