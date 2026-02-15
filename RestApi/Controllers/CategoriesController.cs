using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestApi.Data;
using RestApi.Models;

namespace RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db) => _db = db;

    /// <summary>Liste toutes les catégories</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _db.Categories
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.Products.Count))
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>Récupère une catégorie avec ses produits</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.Products.Count))
            .FirstOrDefaultAsync();

        return category is null ? NotFound(new { message = $"Catégorie {id} introuvable" }) : Ok(category);
    }

    /// <summary>Produits d'une catégorie</summary>
    [HttpGet("{id}/products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(int id)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == id))
            return NotFound(new { message = $"Catégorie {id} introuvable" });

        var products = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == id)
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(
                p.Id, p.Name, p.Description, p.Price, p.Stock,
                p.IsActive, p.CreatedAt, p.CategoryId,
                p.Category!.Name))
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>Crée une catégorie</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            return Conflict(new { message = $"La catégorie '{dto.Name}' existe déjà" });

        var category = new Category { Name = dto.Name, Description = dto.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            new CategoryDto(category.Id, category.Name, category.Description, 0));
    }

    /// <summary>Met à jour une catégorie</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return NotFound(new { message = $"Catégorie {id} introuvable" });

        category.Name = dto.Name;
        category.Description = dto.Description;
        await _db.SaveChangesAsync();

        var count = await _db.Products.CountAsync(p => p.CategoryId == id);
        return Ok(new CategoryDto(category.Id, category.Name, category.Description, count));
    }

    /// <summary>Supprime une catégorie</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return NotFound(new { message = $"Catégorie {id} introuvable" });

        if (await _db.Products.AnyAsync(p => p.CategoryId == id))
            return Conflict(new { message = "Impossible de supprimer une catégorie contenant des produits" });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
