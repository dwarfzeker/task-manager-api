using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace todoApp.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> 
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data source=todos.db");
        
        return new AppDbContext(optionsBuilder.Options);
    }
}