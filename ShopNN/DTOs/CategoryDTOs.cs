namespace ShopNN.DTOs
{
    public class CategoryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class CategoryRequestDTO
    {
        public string Name { get; set; }
    }
}
