using Microsoft.EntityFrameworkCore;
using ShopNN.Entities;
using ShopNN.Repositories.Interface;
using ShopNN.Shared.Exeptions;
using System.Linq.Expressions;

namespace ShopNN.Repositories.Implement
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _db;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        public async Task<T> AddAsync(T data)
        {
            _db.Add(data);
             await _context.SaveChangesAsync();
            return data;
        }

        public async Task DeleteAsync(Guid id)
        {
            T data = await  GetByIdAsync(id) ?? throw new NotFoundException($"Entity with id {id} not found.");
            _db.Remove(data);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _db.Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _db.AsNoTracking().ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _db.FindAsync(id);
        }

        public async Task UpdateAsync(T data)
        {
            _db.Update(data);
            await _context.SaveChangesAsync();
        }
    }
}
