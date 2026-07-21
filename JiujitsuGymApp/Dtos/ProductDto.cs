namespace JiujitsuGymApp.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "general";
        public DateTime CreatedDate { get; set; }
    }
}
