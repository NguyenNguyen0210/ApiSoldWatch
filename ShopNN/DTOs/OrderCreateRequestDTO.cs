using ShopNN.Entities;
using ShopNN.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class OrderCreateRequestDTO
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
