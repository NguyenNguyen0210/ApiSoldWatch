using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNN.Entities;

namespace ShopNN.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasData(
                new Product { Id = 101, Name = "Rolex Day-Date 40", Description = "18ct yellow gold, President bracelet", Price = 38000, Stock = 3, CategoryId = SeedDataConstants.CatLuxuryId, ImageUrl = "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = 102, Name = "Patek Philippe Nautilus", Description = "Steel blue dial, luxury sports watch", Price = 120000, Stock = 1, CategoryId = SeedDataConstants.CatLuxuryId, ImageUrl = "https://images.unsplash.com/photo-1547996160-81dfa63595aa?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = 103, Name = "Audemars Piguet Royal Oak", Description = "Selfwinding 'Jumbo' Extra-thin", Price = 75000, Stock = 2, CategoryId = SeedDataConstants.CatLuxuryId, ImageUrl = "https://images.unsplash.com/photo-1614164185128-e4ec99c436d7?q=80&w=1000&auto=format&fit=crop" },

                new Product { Id = 104, Name = "Casio G-Shock Mudmaster", Description = "Carbon Core Guard, Triple Sensor", Price = 850, Stock = 20, CategoryId = SeedDataConstants.CatSportId, ImageUrl = "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = 105, Name = "Seiko Prospex 'Turtle'", Description = "Automatic diver's watch 200m", Price = 550, Stock = 15, CategoryId = SeedDataConstants.CatSportId, ImageUrl = "https://images.unsplash.com/photo-1612817159949-195b6eb9e31a?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = 106, Name = "Garmin Fenix 7X", Description = "Solar powered multisport GPS watch", Price = 999, Stock = 10, CategoryId = SeedDataConstants.CatSportId, ImageUrl = "https://images.unsplash.com/photo-1517502884422-41eaead166d4?q=80&w=1000&auto=format&fit=crop" },

                new Product { Id = 107, Name = "Apple Watch Ultra 2", Description = "Rugged and capable, with GPS + Cellular", Price = 799, Stock = 25, CategoryId = SeedDataConstants.CatSmartId, ImageUrl = "https://images.unsplash.com/photo-1434493907317-a46b5bc78344?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = 108, Name = "Samsung Galaxy Watch 6", Description = "Advanced sleep tracking and wellness", Price = 350, Stock = 30, CategoryId = SeedDataConstants.CatSmartId, ImageUrl = "https://images.unsplash.com/photo-1508685096489-77a46807e624?q=80&w=1000&auto=format&fit=crop" },

                new Product { Id = 109, Name = "Longines Master Collection", Description = "Elegant moonphase automatic watch", Price = 2500, Stock = 8, CategoryId = SeedDataConstants.CatClassicId, ImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = 110, Name = "Tissot Le Locle", Description = "Traditional swiss automatic watch", Price = 650, Stock = 12, CategoryId = SeedDataConstants.CatClassicId, ImageUrl = "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = 111, Name = "Hamilton Jazzmaster", Description = "Open heart dial, stainless steel", Price = 950, Stock = 7, CategoryId = SeedDataConstants.CatClassicId, ImageUrl = "https://images.unsplash.com/photo-1509048191080-d2984bad6ad5?q=80&w=1000&auto=format&fit=crop" }
            );
        }
    }
}
