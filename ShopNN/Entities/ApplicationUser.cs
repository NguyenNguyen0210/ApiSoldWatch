using Microsoft.AspNetCore.Identity;

namespace ShopNN.Entities
{
    public class ApplicationUser:IdentityUser<Guid>

    {
        public List<Order> Orders { get; set; } = new();
    }
}
