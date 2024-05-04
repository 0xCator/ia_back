using ia_back.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ia_back.Data.Custom_Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _db;
        private readonly DbSet<User> table;

        public UserRepository(DataContext db)
        {
            _db = db;
            table = _db.Set<User>();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            Console.WriteLine("Getting all users");
            return await table.Include(u => u.CreatedProjects)
                .Include(u => u.AssignedProjects)
                .Include(u => u.ProjectRequests)
                .ToListAsync();
        }

        public async Task<User> GetByIdIncludeAsync(int id, params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = table;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var foundUser = await table.FindAsync(id);
            if (foundUser != null)
            {
                await _db.Entry(foundUser).Collection(u => u.CreatedProjects).LoadAsync();
                await _db.Entry(foundUser).Collection(u => u.AssignedProjects).LoadAsync();
                await _db.Entry(foundUser).Collection(u => u.ProjectRequests).LoadAsync();
            }
            return foundUser;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            var foundUser = await table.FirstOrDefaultAsync(u => u.Username == username.ToLower());
            if (foundUser != null)
            {
                await _db.Entry(foundUser).Collection(u => u.CreatedProjects).LoadAsync();
                await _db.Entry(foundUser).Collection(u => u.AssignedProjects).LoadAsync();
                await _db.Entry(foundUser).Collection(u => u.ProjectRequests).LoadAsync();
            }
            return foundUser;
        }

        public async Task<bool> UserExists(string username, string email)
        {
            return await table.AnyAsync(u => u.Username == username || u.Email == email);
        }

        public async Task AddAsync(User entity)
        {
            await table.AddAsync(entity);
        }

        public async Task UpdateAsync(User entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(User entity)
        {
            table.Remove(entity);
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
