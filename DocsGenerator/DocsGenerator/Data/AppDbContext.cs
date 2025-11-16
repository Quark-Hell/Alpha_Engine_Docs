using DocsGenerator.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsGenerator.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ProjectVersion> ProjectVersions => Set<ProjectVersion>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectVersion>(eb =>
            {
                eb.HasIndex(e => e.CommitHash).IsUnique();
                eb.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });
        }
    }
}
