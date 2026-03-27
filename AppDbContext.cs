using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<ZaloTokenEntity> ZaloTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
