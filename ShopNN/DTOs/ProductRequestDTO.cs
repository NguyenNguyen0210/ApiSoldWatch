using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
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

        public Guid? CategoryId { get; set; }
    }
}
