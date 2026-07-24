using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using JiujitsuGymApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Tests.Services;

public sealed class ProductServiceTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Product> SeedProductAsync(string name = "Gi", decimal price = 99.99m)
    {
        using var context = _db.CreateContext();
        var product = new Product { Name = name, Price = price, Description = "", Category = "apparel" };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsAllProductsOrderedByName()
    {
        await SeedProductAsync("Rash Guard");
        await SeedProductAsync("Belt");
        await SeedProductAsync("Gi");

        using var context = _db.CreateContext();
        var service = new ProductService(context);

        var result = await service.GetProductsAsync();

        Assert.Equal(new[] { "Belt", "Gi", "Rash Guard" }, result.Select(p => p.Name));
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ReturnsDto()
    {
        var seeded = await SeedProductAsync("Gi", 149.50m);

        using var context = _db.CreateContext();
        var service = new ProductService(context);

        var result = await service.GetProductByIdAsync(seeded.Id);

        Assert.NotNull(result);
        Assert.Equal("Gi", result.Name);
        Assert.Equal(149.50m, result.Price);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        using var context = _db.CreateContext();
        var service = new ProductService(context);

        var result = await service.GetProductByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateProductAsync_PersistsProductAndSetsCreatedDate()
    {
        using var context = _db.CreateContext();
        var service = new ProductService(context);
        var dto = new CreateProductDto { Name = "Mouthguard", Price = 19.99m, Category = "gear" };

        var result = await service.CreateProductAsync(dto);

        using var assertContext = _db.CreateContext();
        var saved = await assertContext.Products.SingleAsync(p => p.Id == result.Id);
        Assert.Equal("Mouthguard", saved.Name);
        Assert.Equal(19.99m, saved.Price);
        Assert.NotEqual(default, saved.CreatedDate);
    }

    [Fact]
    public async Task CreateProductAsync_WithNullDescription_StoresEmptyString()
    {
        using var context = _db.CreateContext();
        var service = new ProductService(context);
        var dto = new CreateProductDto { Name = "Tape", Price = 5.00m, Description = null };

        var result = await service.CreateProductAsync(dto);

        Assert.Equal(string.Empty, result.Description);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductExists_UpdatesFieldsAndReturnsTrue()
    {
        var seeded = await SeedProductAsync("Gi", 100.00m);

        using var context = _db.CreateContext();
        var service = new ProductService(context);
        var dto = new UpdateProductDto { Name = "Competition Gi", Price = 120.00m, Category = "apparel" };

        var updated = await service.UpdateProductAsync(seeded.Id, dto);

        Assert.True(updated);
        using var assertContext = _db.CreateContext();
        var saved = await assertContext.Products.SingleAsync(p => p.Id == seeded.Id);
        Assert.Equal("Competition Gi", saved.Name);
        Assert.Equal(120.00m, saved.Price);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductDoesNotExist_ReturnsFalse()
    {
        using var context = _db.CreateContext();
        var service = new ProductService(context);
        var dto = new UpdateProductDto { Name = "Anything", Price = 1.00m };

        var updated = await service.UpdateProductAsync(999, dto);

        Assert.False(updated);
    }

    [Fact]
    public async Task ProductExistsAsync_ReflectsDatabaseState()
    {
        var seeded = await SeedProductAsync();

        using var context = _db.CreateContext();
        var service = new ProductService(context);

        Assert.True(await service.ProductExistsAsync(seeded.Id));
        Assert.False(await service.ProductExistsAsync(999));
    }
}
