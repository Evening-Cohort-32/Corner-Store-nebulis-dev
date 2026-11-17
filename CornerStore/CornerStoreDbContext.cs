using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
using System.Security.Cryptography.X509Certificates;
public class CornerStoreDbContext : DbContext
{

    public DbSet <Cashier>      Cashiers        { get; set; }
    public DbSet <Product>      Products        { get; set; }
    public DbSet <Category>     Categories      { get; set; }
    public DbSet <Order>        Orders          { get; set; }
    public DbSet <OrderProduct> OrderProducts   { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {
        
    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    
        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "Snacks" },
            new Category { Id = 2, CategoryName = "Beverages" },
            new Category { Id = 3, CategoryName = "Candy" },
            new Category { Id = 4, CategoryName = "Household" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, ProductName = "Potato Chips",    Price = 2.49m, Brand = "Lays",      CategoryId = 1 },
            new Product { Id = 2, ProductName = "Tortilla Chips",  Price = 3.29m, Brand = "Doritos",   CategoryId = 1 },
            new Product { Id = 3, ProductName = "Cola",            Price = 1.99m, Brand = "Coca-Cola", CategoryId = 2 },
            new Product { Id = 4, ProductName = "Orange Juice",    Price = 3.99m, Brand = "Tropicana", CategoryId = 2 },
            new Product { Id = 5, ProductName = "Chocolate Bar",   Price = 1.49m, Brand = "Hershey's", CategoryId = 3 },
            new Product { Id = 6, ProductName = "Gummy Bears",     Price = 2.19m, Brand = "Haribo",    CategoryId = 3 },
            new Product { Id = 7, ProductName = "Paper Towels",    Price = 5.99m, Brand = "Bounty",    CategoryId = 4 }
        );

        modelBuilder.Entity<Cashier>().HasData(
            new Cashier { Id = 1, FirstName = "Alice", LastName = "Johnson" },
            new Cashier { Id = 2, FirstName = "Marcus", LastName = "Lee" },
            new Cashier { Id = 3, FirstName = "Priya", LastName = "Patel" }
        );

        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, CashierId = 1, OrderDate = new DateTime(2026, 11, 14, 9, 15, 0),  PaidOnDate = new DateTime(2026, 11, 14, 9, 20, 0) },
            new Order { Id = 2, CashierId = 2, OrderDate = new DateTime(2026, 11, 15, 13, 40, 0), PaidOnDate = new DateTime(2026, 11, 15, 13, 45, 0) },
            new Order { Id = 3, CashierId = 1, OrderDate = new DateTime(2026, 11, 16, 8, 5, 0),   PaidOnDate = null },
            new Order { Id = 4, CashierId = 3, OrderDate = new DateTime(2026, 11, 16, 16, 30, 0), PaidOnDate = new DateTime(2026, 11, 16, 16, 35, 0) }
        );

        modelBuilder.Entity<OrderProduct>().HasData(
            new OrderProduct { OrderId = 1, ProductId = 1, Quantity = 2 },
            new OrderProduct { OrderId = 1, ProductId = 3, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 4, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 7, Quantity = 2 },
            new OrderProduct { OrderId = 3, ProductId = 5, Quantity = 3 },
            new OrderProduct { OrderId = 3, ProductId = 6, Quantity = 1 },
            new OrderProduct { OrderId = 4, ProductId = 2, Quantity = 1 },
            new OrderProduct { OrderId = 4, ProductId = 3, Quantity = 2 }
        );
    }
}