namespace ShopNN.DTOs
{
    public class OrderItemResponseDTO
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}