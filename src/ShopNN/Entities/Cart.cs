namespace ShopNN.Entities
{
    public class Cart
    {
        public Guid Id { get; set; }


        public Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }

        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<CartItem> Items { get; set; } = new();
    }
}
