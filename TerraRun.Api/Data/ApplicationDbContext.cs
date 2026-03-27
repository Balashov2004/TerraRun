
using Microsoft.EntityFrameworkCore;
using TerraRun.Api.Models;

namespace TerraRun.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Run> Runs { get; set; }
    public DbSet<RunPoint> RunPoints { get; set; }
    public DbSet<CapturedCell> CapturedCells { get; set; }
}