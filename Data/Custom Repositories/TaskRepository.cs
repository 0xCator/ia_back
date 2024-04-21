using ia_back.Models;
using Microsoft.EntityFrameworkCore;

namespace ia_back.Data.Custom_Repositories
{
    public class TaskRepository : IDataRepository<ProjectTask>
    {
        private readonly DataContext _db;
        private readonly DbSet<ProjectTask> table;

        public TaskRepository(DataContext db)
        {
            _db = db;
            table = _db.Set<ProjectTask>();
        }

        public async Task<IEnumerable<ProjectTask>> GetAllAsync()
        {
            return await table
                .Include(t => t.Project)
                .Include(t => t.AssignedDev)
                .Include(t => t.Comments)
                .ToListAsync();
        }

        public async Task<ProjectTask> GetByIdAsync(int id)
        {
            var foundTask = await table.FindAsync(id);
            if (foundTask != null)
            {
                await _db.Entry(foundTask).Reference(t => t.Project).LoadAsync();
                await _db.Entry(foundTask).Reference(t => t.AssignedDev).LoadAsync();
                await _db.Entry(foundTask).Collection(t => t.Comments).LoadAsync();
            }
            return foundTask;
        }

        public async Task AddAsync(ProjectTask entity)
        {
            await table.AddAsync(entity);
        }

        public async Task UpdateAsync(ProjectTask entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(ProjectTask entity)
        {
            table.Remove(entity);
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
