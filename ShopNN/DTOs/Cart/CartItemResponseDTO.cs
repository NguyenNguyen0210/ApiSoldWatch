namespace ShopNN.DTOs.Cart
{
    public class CartItemResponseDTO
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
    }
}
