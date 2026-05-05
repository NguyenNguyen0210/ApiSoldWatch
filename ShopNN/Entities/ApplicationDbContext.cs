using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace ShopNN.Entities
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> Items { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            model.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new ApplicationRole { Id = userRoleId, Name = "User", NormalizedName = "USER" }
            );

            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            model.Entity<ApplicationUser>().HasData(adminUser);

            model.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid> { UserId = adminUserId, RoleId = adminRoleId }
            );

            var catLuxuryId = Guid.Parse("c1111111-1111-1111-1111-111111111111");
            var catSportId = Guid.Parse("c2222222-2222-2222-2222-222222222222");
            var catSmartId = Guid.Parse("c3333333-3333-3333-3333-333333333333");
            var catClassicId = Guid.Parse("c4444444-4444-4444-4444-444444444444");

            model.Entity<Category>().HasData(
                new Category { Id = catLuxuryId, Name = "Luxury Watches" },
                new Category { Id = catSportId, Name = "Sport Watches" },
                new Category { Id = catSmartId, Name = "Smart Watches" },
                new Category { Id = catClassicId, Name = "Classic Watches" }
            );

            model.Entity<Product>().HasData(
                new Product { Id = Guid.NewGuid(), Name = "Rolex Day-Date 40", Description = "18ct yellow gold, President bracelet", Price = 38000, Stock = 3, CategoryId = catLuxuryId, ImageUrl = "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = Guid.NewGuid(), Name = "Patek Philippe Nautilus", Description = "Steel blue dial, luxury sports watch", Price = 120000, Stock = 1, CategoryId = catLuxuryId, ImageUrl = "https://images.unsplash.com/photo-1547996160-81dfa63595aa?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = Guid.NewGuid(), Name = "Audemars Piguet Royal Oak", Description = "Selfwinding 'Jumbo' Extra-thin", Price = 75000, Stock = 2, CategoryId = catLuxuryId, ImageUrl = "https://images.unsplash.com/photo-1614164185128-e4ec99c436d7?q=80&w=1000&auto=format&fit=crop" },

                new Product { Id = Guid.NewGuid(), Name = "Casio G-Shock Mudmaster", Description = "Carbon Core Guard, Triple Sensor", Price = 850, Stock = 20, CategoryId = catSportId, ImageUrl = "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = Guid.NewGuid(), Name = "Seiko Prospex 'Turtle'", Description = "Automatic diver's watch 200m", Price = 550, Stock = 15, CategoryId = catSportId, ImageUrl = "https://images.unsplash.com/photo-1612817159949-195b6eb9e31a?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = Guid.NewGuid(), Name = "Garmin Fenix 7X", Description = "Solar powered multisport GPS watch", Price = 999, Stock = 10, CategoryId = catSportId, ImageUrl = "https://images.unsplash.com/photo-1517502884422-41eaead166d4?q=80&w=1000&auto=format&fit=crop" },

                new Product { Id = Guid.NewGuid(), Name = "Apple Watch Ultra 2", Description = "Rugged and capable, with GPS + Cellular", Price = 799, Stock = 25, CategoryId = catSmartId, ImageUrl = "https://images.unsplash.com/photo-1434493907317-a46b5bc78344?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = Guid.NewGuid(), Name = "Samsung Galaxy Watch 6", Description = "Advanced sleep tracking and wellness", Price = 350, Stock = 30, CategoryId = catSmartId, ImageUrl = "https://images.unsplash.com/photo-1508685096489-77a46807e624?q=80&w=1000&auto=format&fit=crop" },

                new Product { Id = Guid.NewGuid(), Name = "Longines Master Collection", Description = "Elegant moonphase automatic watch", Price = 2500, Stock = 8, CategoryId = catClassicId, ImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = Guid.NewGuid(), Name = "Tissot Le Locle", Description = "Traditional swiss automatic watch", Price = 650, Stock = 12, CategoryId = catClassicId, ImageUrl = "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?q=80&w=1000&auto=format&fit=crop" },
                new Product { Id = Guid.NewGuid(), Name = "Hamilton Jazzmaster", Description = "Open heart dial, stainless steel", Price = 950, Stock = 7, CategoryId = catClassicId, ImageUrl = "https://images.unsplash.com/photo-1509048191080-d2984bad6ad5?q=80&w=1000&auto=format&fit=crop" }
            );

            model.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany() 
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            model.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            model.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            model.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            model.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            model.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            model.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");


            model.Entity<OrderItem>()
                .Property(x => x.Quantity)
                .IsRequired();

            model.Entity<RefreshToken>()
    .HasOne(rt => rt.User)
    .WithMany()   
    .HasForeignKey(rt => rt.UserId)
    .OnDelete(DeleteBehavior.Cascade);

            model.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);


            model.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            model.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            model.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            model.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne()
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
