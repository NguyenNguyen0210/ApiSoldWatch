using ShopNN.Entities;

public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; }           // ✔ navigation

    public Guid ProductId { get; set; }
    public Product Product { get; set; }       // ✔ navigation

    public int Quantity { get; set; }          // fix typo
}