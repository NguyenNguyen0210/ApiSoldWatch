using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs.Product
{
    public class ProductRequestDTO
    {
        [Required]
        public required string Name { get; set; }
        
        [Required]
        public required string Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        public int? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
