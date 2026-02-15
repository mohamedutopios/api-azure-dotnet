using Microsoft.EntityFrameworkCore;
using RestApi.Models;

namespace RestApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Index unique sur le nom de catégorie
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Relation Product -> Category
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Électronique", Description = "Appareils et gadgets électroniques" },
            new Category { Id = 2, Name = "Livres", Description = "Livres papier et numériques" },
            new Category { Id = 3, Name = "Vêtements", Description = "Mode et accessoires" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop Pro 15", Description = "Ordinateur portable 15 pouces, 16Go RAM, 512Go SSD", Price = 1299.99m, Stock = 25, CategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Clavier Mécanique RGB", Description = "Clavier gaming switches Cherry MX", Price = 89.99m, Stock = 150, CategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "Clean Code", Description = "Robert C. Martin - Guide du code propre", Price = 34.50m, Stock = 80, CategoryId = 2, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, Name = "Design Patterns", Description = "Gang of Four - Patterns de conception", Price = 42.00m, Stock = 45, CategoryId = 2, CreatedAt = DateTime.UtcNow },
            new Product { Id = 5, Name = "T-Shirt .NET", Description = "T-shirt développeur .NET, 100% coton", Price = 24.99m, Stock = 200, CategoryId = 3, CreatedAt = DateTime.UtcNow }
        );
    }
}
