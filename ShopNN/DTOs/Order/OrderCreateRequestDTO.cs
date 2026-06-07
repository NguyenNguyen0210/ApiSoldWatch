using ShopNN.Entities;
using ShopNN.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs.Order
{
    public class OrderCreateRequestDTO
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Receiver name is required.")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping address is required.")]
        public string ShippingAddress { get; set; } = string.Empty;
    }
}
