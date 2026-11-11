using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
using System.Security.Cryptography.X509Certificates;
public class CornerStoreDbContext : DbContext
{

    public DbSet<Cashier>Cashiers { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {
        
    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}