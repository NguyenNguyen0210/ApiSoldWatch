using ShopNN.Entities;
using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class CheckoutRequestDTO
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
