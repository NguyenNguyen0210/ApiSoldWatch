using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNN.Entities;
using System.Text.Json;

namespace ShopNN.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "SeedData", "products.json");
            var json = File.ReadAllText(jsonPath);
            var products = JsonSerializer.Deserialize<List<Product>>(json)!;
            builder.HasData(products);
        }
    }
}
