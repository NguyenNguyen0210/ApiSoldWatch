using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class CartItemUpdateDTO
    {
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }
    }
}
