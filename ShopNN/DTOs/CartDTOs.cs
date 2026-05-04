namespace ShopNN.DTOs
{
    public class CartDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemDTO> Items { get; set; } = new();
    }

    public class CartItemDTO
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class AddToCartDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDTO
    {
        public int Quantity { get; set; }
    }
}
