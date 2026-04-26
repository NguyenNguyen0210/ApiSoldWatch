using System.ComponentModel.DataAnnotations;

namespace ShopNN.DTOs
{
    public class OrderItemDTO
    {
            [Required()]    
            public Guid ProductId { get; set; }
        [Required()]

        public int Quantity { get; set; }

    }
}