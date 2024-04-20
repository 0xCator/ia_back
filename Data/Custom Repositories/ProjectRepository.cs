using ia_back.Models;
using Microsoft.EntityFrameworkCore;

namespace ia_back.Data.Custom_Repositories
{
    public class ProjectRepository : IDataRepository<Project>
    {
        private readonly DataContext _db;
        private readonly DbSet<Project> table;

        public ProjectRepository(DataContext db)
        {
            _db = db;
            table = _db.Set<Project>();
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await table
                .Include(p => p.TeamLeader)
                .Include(p => p.RequestedDevelopers)
                .Include(p => p.AssignedDevelopers)
                .ToListAsync();
        }

        public async Task<Project> GetByIdAsync(int id)
        {
            var foundProject = await table.FindAsync(id);
            if (foundProject != null)
            {
                await _db.Entry(foundProject).Reference(p => p.TeamLeader).LoadAsync();
                await _db.Entry(foundProject).Collection(p => p.RequestedDevelopers).LoadAsync();
                await _db.Entry(foundProject).Collection(p => p.AssignedDevelopers).LoadAsync();
            }
            return foundProject;
        }

        public async Task AddAsync(Project entity)
        {
            await table.AddAsync(entity);
        }

        public async Task UpdateAsync(Project entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Project entity)
        {
            table.Remove(entity);
        }

        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
