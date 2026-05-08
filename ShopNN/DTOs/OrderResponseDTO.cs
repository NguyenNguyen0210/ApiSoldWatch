using System.Text.Json.Serialization;

namespace ShopNN.DTOs
{
    public class OrderResponseDTO
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PaymentUrl { get; set; }
        
        public List<OrderItemResponseDTO> Items { get; set; } = new();
    }
}
