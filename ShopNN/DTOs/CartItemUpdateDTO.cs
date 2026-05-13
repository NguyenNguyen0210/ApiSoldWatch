using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class CartItemUpdateDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
