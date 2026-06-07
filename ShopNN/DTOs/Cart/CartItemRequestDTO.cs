using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs.Cart
{
    public class CartItemRequestDTO
    {
        [Required]
        public int ProductId { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
