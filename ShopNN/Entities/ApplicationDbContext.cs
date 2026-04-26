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
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            // ===== FIXED GUID =====
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            var p1 = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
            var p2 = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
            var p3 = Guid.Parse("cccccccc-3333-3333-3333-333333333333");

            // ===== ROLE =====
            model.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new ApplicationRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER"
                }
            );

            // ===== ADMIN USER =====
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

            // ===== USER ROLE MAPPING =====
            model.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid>
                {
                    UserId = adminUserId,
                    RoleId = adminRoleId
                }
            );

            // ===== PRODUCTS (WATCH) =====
            model.Entity<Product>().HasData(
                new Product
                {
                    Id = p1,
                    Name = "Rolex Submariner",
                    Description = "Luxury diving watch",
                    Price = 15000,
                    Stock = 5
                },
                new Product
                {
                    Id = p2,
                    Name = "Omega Speedmaster",
                    Description = "Moonwatch легендарный",
                    Price = 8000,
                    Stock = 10
                },
                new Product
                {
                    Id = p3,
                    Name = "Casio G-Shock",
                    Description = "Durable sport watch",
                    Price = 150,
                    Stock = 50
                }
            );
            model.Entity<Order>()
     .HasOne(o => o.User)
     .WithMany() // nếu bạn không có ICollection<Order> trong ApplicationUser
     .HasForeignKey(o => o.UserId)
     .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // ORDER - ORDERITEM (1 - N)
            // =========================
            model.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // PRODUCT - ORDERITEM (1 - N)
            // =========================
            model.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // FIX: decimal precision (SQL Server)
            // =========================
            model.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // =========================
            // FIX: typo (optional safety)
            // =========================
            model.Entity<OrderItem>()
                .Property(x => x.Quantity)
                .IsRequired();

            model.Entity<RefreshToken>()
    .HasOne(rt => rt.User)
    .WithMany()   // cần có navigation bên User
    .HasForeignKey(rt => rt.UserId)
    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
