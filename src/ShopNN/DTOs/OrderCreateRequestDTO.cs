using ShopNN.Entities;
using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class OrderCreateRequestDTO
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
