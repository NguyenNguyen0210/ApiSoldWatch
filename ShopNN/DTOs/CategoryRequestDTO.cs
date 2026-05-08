using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class CategoryRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
