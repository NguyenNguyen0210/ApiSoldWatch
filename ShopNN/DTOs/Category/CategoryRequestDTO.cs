using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs.Category
{
    public class CategoryRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
