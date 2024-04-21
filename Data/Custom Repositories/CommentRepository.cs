using ia_back.Models;
using Microsoft.EntityFrameworkCore;

namespace ia_back.Data.Custom_Repositories
{
    public class CommentRepository : IDataRepository<Comment>
    {
        private readonly DataContext _db;
        private readonly DbSet<Comment> table;

        public CommentRepository(DataContext db)
        {
            _db = db;
            table = _db.Set<Comment>();
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await table.Include(c => c.User)
                .Include(c => c.ParentComment)
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<Comment> GetByIdAsync(int id)
        {
            var foundComment = await table.FindAsync(id);
            if (foundComment != null)
            {
                await _db.Entry(foundComment).Reference(c => c.User).LoadAsync();
                await _db.Entry(foundComment).Reference(c => c.ParentComment).LoadAsync();
            }
            return foundComment;
        }

        public async Task AddAsync(Comment entity)
        {
            await table.AddAsync(entity);
        }

        public async Task UpdateAsync(Comment entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Comment entity)
        {
            table.Remove(entity);
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}