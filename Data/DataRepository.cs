using ia_back.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ia_back.Data
{
    public class DataRepository<T> : IDataRepository<T> where T : class
    {
        private readonly DataContext _db;
        private readonly DbSet<T> table;

        public DataRepository(DataContext db)
        {
            _db = db;
            table = _db.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await table.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllIncludeAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = table;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllIncludeCriteriaAsync(Expression<Func<T, bool>> idExpression, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = table;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return await query.Where(idExpression).ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await table.FindAsync(id);
        }

        public async Task<T> GetByIdIncludeAsync(Expression<Func<T, bool>> idExpression, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = table;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return await query.FirstOrDefaultAsync(idExpression);
        }

        public async Task AddAsync(T entity)
        {
            await table.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(T entity)
        {
            table.Remove(entity);
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
