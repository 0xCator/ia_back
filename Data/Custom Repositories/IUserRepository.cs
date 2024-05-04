using ia_back.Models;
using System.Linq.Expressions;

namespace ia_back.Data.Custom_Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByIdIncludeAsync(int id, params Expression<Func<User, object>>[] includes);
        Task<User> GetByUsernameAsync(string username);
        Task<bool> UserExists(string username, string email); 
        Task AddAsync(User entity);
        Task UpdateAsync(User entity);
        Task DeleteAsync(User entity);
        Task<bool> Save();
    }
}
