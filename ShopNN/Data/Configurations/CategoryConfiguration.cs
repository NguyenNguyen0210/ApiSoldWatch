using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNN.Entities;

namespace ShopNN.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasData(
                new Category { Id = SeedDataConstants.CatLuxuryId, Name = "Luxury Watches" },
                new Category { Id = SeedDataConstants.CatSportId, Name = "Sport Watches" },
                new Category { Id = SeedDataConstants.CatSmartId, Name = "Smart Watches" },
                new Category { Id = SeedDataConstants.CatClassicId, Name = "Classic Watches" }
            );
        }
    }
}
