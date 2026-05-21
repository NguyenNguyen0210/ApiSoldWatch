using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNN.Entities;
using System.Text.Json;

namespace ShopNN.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "SeedData", "categories.json");
            var json = File.ReadAllText(jsonPath);
            var categories = JsonSerializer.Deserialize<List<Category>>(json)!;
            builder.HasData(categories);
        }
    }
}
