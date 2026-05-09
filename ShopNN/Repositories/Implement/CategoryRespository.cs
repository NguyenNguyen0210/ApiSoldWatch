using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Repositories.Implement;
namespace ShopNN.Repositories.Implement
{
    public class CategoryRespository : GenericRepository<Category>, ICategoryRespository
    {
        public CategoryRespository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
