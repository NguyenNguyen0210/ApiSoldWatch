using Microsoft.EntityFrameworkCore;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Repositories.Implement
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async override Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.AsNoTracking().Include(p => p.Category).ToListAsync();
        }

        public async override Task<Product?> GetByIdAsync(object id)
        {
            int intId = id is int i ? i : int.Parse(id.ToString()!);
            return await _context.Products.AsNoTracking().Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == intId);
        }

        public async Task<PagedResult<Product>> GetPagedAsync(ProductQueryDTO query)
        {
            var q = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                q = q.Where(p => p.Name.ToLower().Contains(search)
                               || p.Description.ToLower().Contains(search));
            }

            // Filter by Category
            if (query.CategoryId.HasValue)
                q = q.Where(p => p.CategoryId == query.CategoryId.Value);

            // Filter by Price range
            if (query.MinPrice.HasValue)
                q = q.Where(p => p.Price >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue)
                q = q.Where(p => p.Price <= query.MaxPrice.Value);

            // Filter by Stock
            if (query.InStock.HasValue)
                q = query.InStock.Value
                    ? q.Where(p => p.Stock > 0)
                    : q.Where(p => p.Stock == 0);

            // Sort
            q = (query.SortBy?.ToLower()) switch
            {
                "price" => query.SortOrder?.ToLower() == "desc"
                    ? q.OrderByDescending(p => p.Price)
                    : q.OrderBy(p => p.Price),
                "date" or "createdat" => query.SortOrder?.ToLower() == "desc"
                    ? q.OrderByDescending(p => p.Id)
                    : q.OrderBy(p => p.Id),
                "stock" => query.SortOrder?.ToLower() == "desc"
                    ? q.OrderByDescending(p => p.Stock)
                    : q.OrderBy(p => p.Stock),
                _ => query.SortOrder?.ToLower() == "desc"
                    ? q.OrderByDescending(p => p.Name)
                    : q.OrderBy(p => p.Name)
            };

            var totalCount = await q.CountAsync();

            var items = await q
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
