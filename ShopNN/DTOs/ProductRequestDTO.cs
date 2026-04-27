using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class ProductRequestDTO
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
