using ShopNN.Entities;

public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; }   // ✔ navigation

    public DateTime CreatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}