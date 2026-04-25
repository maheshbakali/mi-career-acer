using Microsoft.EntityFrameworkCore;
using MiCareerAcer.Api.Models;

namespace MiCareerAcer.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<AgentSession> AgentSessions => Set<AgentSession>();
    public DbSet<ResumeFile> ResumeFiles => Set<ResumeFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(320);
        });

        modelBuilder.Entity<Job>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Company).HasMaxLength(300);
            e.Property(x => x.Location).HasMaxLength(300);
            e.Property(x => x.Url).HasMaxLength(2000);
            e.Property(x => x.Description);
        });

        modelBuilder.Entity<ResumeFile>(e =>
        {
            e.Property(x => x.FileName).HasMaxLength(500);
            e.Property(x => x.StoragePath).HasMaxLength(2000);
        });

        modelBuilder.Entity<AgentSession>(e =>
        {
            e.Property(x => x.InputPayload);
            e.Property(x => x.OutputPayload);
            e.Property(x => x.CompatibilityScore).HasPrecision(6, 2);
        });
    }
}
