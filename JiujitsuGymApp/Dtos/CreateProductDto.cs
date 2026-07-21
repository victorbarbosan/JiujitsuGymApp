using System.ComponentModel.DataAnnotations;

namespace JiujitsuGymApp.Dtos
{
    public class CreateProductDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = "general";
    }
}
