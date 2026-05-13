using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNN.Entities;
using ShopNN.Shared.Enums;

namespace ShopNN.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.HasData(
                new ApplicationRole 
                { 
                    Id = SeedDataConstants.AdminRoleId, 
                    Name = RoleNames.Admin, 
                    NormalizedName = RoleNames.Admin.ToUpper() 
                },
                new ApplicationRole 
                { 
                    Id = SeedDataConstants.UserRoleId, 
                    Name = RoleNames.User, 
                    NormalizedName = RoleNames.User.ToUpper() 
                }
            );
        }
    }
}
