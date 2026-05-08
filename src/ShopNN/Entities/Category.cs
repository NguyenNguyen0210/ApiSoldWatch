namespace ShopNN.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        public List<Product> Products { get; set; } = new();
    }
}
