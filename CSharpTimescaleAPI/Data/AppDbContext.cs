using CSharpTimescaleAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CSharpTimescaleAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ValueEntity> Values { get; set; }
    public DbSet<ResultEntity> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ValueEntity>()
            .HasIndex(v => new { v.FileName, v.Date });
    }
}