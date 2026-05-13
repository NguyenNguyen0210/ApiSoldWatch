using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShopNN.DTOs;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Shared.Wrappers;

namespace ShopNN.Repositories.Implement
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async override Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                   .Include(o => o.Items)
                       .ThenInclude(i => i.Product)
                   .OrderByDescending(o => o.CreatedAt)
                   .AsNoTracking()
                   .ToListAsync();
        }

        public override async Task<Order?> GetByIdAsync(object id)
        {
            Guid guidId = id is Guid g ? g : Guid.Parse(id.ToString()!);
            return await _context.Orders
                   .Include(o => o.Items)
                       .ThenInclude(i => i.Product)
                   .FirstOrDefaultAsync(o => o.Id == guidId);
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId) =>
           await _context.Orders
               .Include(o => o.Items)
                   .ThenInclude(i => i.Product)
               .Where(o => o.UserId == userId)
               .OrderByDescending(o => o.CreatedAt)
               .AsNoTracking()
               .ToListAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<PagedResult<Order>> GetPagedAsync(OrderQueryDTO query)
        {
            var q = _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            // Filter by Status
            if (query.Status.HasValue)
                q = q.Where(o => o.Status == query.Status.Value);

            // Filter by PaymentStatus
            if (query.PaymentStatus.HasValue)
                q = q.Where(o => o.PaymentStatus == query.PaymentStatus.Value);

            // Filter by PaymentMethod
            if (query.PaymentMethod.HasValue)
                q = q.Where(o => o.PaymentMethod == query.PaymentMethod.Value);

            // Filter by Date range
            if (query.FromDate.HasValue)
                q = q.Where(o => o.CreatedAt >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                q = q.Where(o => o.CreatedAt <= query.ToDate.Value);

            // Sort
            q = (query.SortBy?.ToLower()) switch
            {
                "amount" or "total" => query.SortOrder?.ToLower() == "asc"
                    ? q.OrderBy(o => o.TotalAmount)
                    : q.OrderByDescending(o => o.TotalAmount),
                "status" => query.SortOrder?.ToLower() == "asc"
                    ? q.OrderBy(o => o.Status)
                    : q.OrderByDescending(o => o.Status),
                _ => query.SortOrder?.ToLower() == "asc"
                    ? q.OrderBy(o => o.CreatedAt)
                    : q.OrderByDescending(o => o.CreatedAt)
            };

            var totalCount = await q.CountAsync();

            var items = await q
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
