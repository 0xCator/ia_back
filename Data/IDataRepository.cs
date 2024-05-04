using System.Linq.Expressions;

namespace ia_back.Data
{
    public interface IDataRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllIncludeAsync(params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllIncludeCriteriaAsync(Expression<Func<T, bool>> idExpression, params Expression<Func<T, object>>[] includes);
        Task<T> GetByIdAsync(int id);
        Task<T> GetByIdIncludeAsync(Expression<Func<T, bool>> idExpression, params Expression<Func<T, object>>[] includes);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> Save();
    }
}
