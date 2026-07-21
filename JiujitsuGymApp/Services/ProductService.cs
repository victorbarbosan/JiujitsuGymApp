using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Services
{
    public class ProductService(ApplicationDbContext db)
    {
        public async Task<List<ProductDto>> GetProductsAsync()
        {
            var products = await db.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(ToDto).ToList();
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return product is null ? null : ToDto(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description ?? string.Empty,
                Category = dto.Category,
                CreatedDate = DateTime.UtcNow
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            return ToDto(product);
        }

        public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return false;

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Description = dto.Description ?? string.Empty;
            product.Category = dto.Category;

            await db.SaveChangesAsync();
            return true;
        }

        public Task<bool> ProductExistsAsync(int id) => db.Products.AnyAsync(p => p.Id == id);

        public static ProductDto ToDto(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Description = p.Description,
            Category = p.Category,
            CreatedDate = p.CreatedDate
        };
    }
}
