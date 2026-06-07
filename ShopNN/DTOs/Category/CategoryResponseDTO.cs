namespace ShopNN.DTOs.Category
{
    public class CategoryResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
