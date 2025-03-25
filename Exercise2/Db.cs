using Microsoft.EntityFrameworkCore;

namespace Exercise1;

public class Db : DbContext
{
    public Db(DbContextOptions<Db> options) : base(options)
    {
        
    }
    
    public DbSet<TimeseriesData> TimeseriesData { get; set; } = null!;
    
}