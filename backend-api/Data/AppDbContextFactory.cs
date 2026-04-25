using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MiCareerAcer.Api.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        var cs = Environment.GetEnvironmentVariable("MICAREER_DB") ?? "Host=localhost;Port=5432;Database=micareeracer;Username=postgres;Password=postgres";
        options.UseNpgsql(cs);
        return new AppDbContext(options.Options);
    }
}
