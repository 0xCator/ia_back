using Microsoft.EntityFrameworkCore;
using ia_back.Models;

namespace ia_back.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> User { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<ProjectTask> ProjectTask { get; set; }
        public DbSet<Comment> Comment { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().
                HasMany(u => u.CreatedProjects)
                .WithOne(p => p.TeamLeader)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.AssignedProjects)
                .WithMany(p => p.AssignedDevelopers)
                .UsingEntity(join => join.ToTable("ProjectMembers"));
            modelBuilder.Entity<User>().
                HasMany(u => u.ProjectRequests)
                .WithMany(p => p.RequestedDevelopers)
                .UsingEntity(join => join.ToTable("Requests")); ;
        }
    }
}
