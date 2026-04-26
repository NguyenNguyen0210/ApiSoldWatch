namespace ShopNN.DTOs
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDTO> Items { get; set; }
    }
}
