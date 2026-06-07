namespace ShopNN.DTOs.Cart
{
    public class CartResponseDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemResponseDTO> Items { get; set; } = new();
    }
}
