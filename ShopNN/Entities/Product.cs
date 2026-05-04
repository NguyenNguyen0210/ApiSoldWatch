namespace ShopNN.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public Guid? CategoryId { get; set; }
        public Category Category { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}