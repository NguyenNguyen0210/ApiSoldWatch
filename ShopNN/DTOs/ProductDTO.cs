using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class ProductDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]

        public string Description { get; set; }
        [Required]

        public decimal Price { get; set; }
        [Required]

        public int Stock { get; set; }
    }
}
