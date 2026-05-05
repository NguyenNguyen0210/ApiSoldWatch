using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class OrderItemDTO
    {
        [Required]    
        public Guid ProductId { get; set; }
        
        public string? ProductName { get; set; }
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}