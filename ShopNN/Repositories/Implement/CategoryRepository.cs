using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Repositories.Implement;
namespace ShopNN.Repositories.Implement
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
