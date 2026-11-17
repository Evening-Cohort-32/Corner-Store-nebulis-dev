using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["ConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here

//Cashier Endpoints

//Add Cashier
app.MapPost("/cashiers", (CornerStoreDbContext db, Cashier cashier) =>
{
    db.Cashiers.Add(cashier);
    db.SaveChanges();
    return Results.Created($"/cashiers/{cashier.Id}", cashier);
});

//Get a cashier by Id
app.MapGet("/cashiers/{id}", (CornerStoreDbContext db, int id) =>
{


    Cashier? cashier = db.Cashiers
    .Where(c => c.Id == id)
    .Select(c => new Cashier
    {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Orders = c.Orders.Select(o => new Order
            {
                Id = o.Id,
                CashierId = o.CashierId,
                OrderDate = o.OrderDate,
                PaidOnDate = o.PaidOnDate,
                OrderProducts = o.OrderProducts.Select(op => new OrderProduct
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    Quantity = op.Quantity,
                    Product = new Product
                    {
                        Id = op.Product.Id,
                        ProductName = op.Product.ProductName,
                        Price = op.Product.Price,
                        Brand = op.Product.Brand,
                        CategoryId = op.Product.CategoryId
                    }
                }).ToList()
            }).ToList()
        })
        .SingleOrDefault();

        return cashier is null ? Results.NotFound() : Results.Ok(cashier);
});

//Product Endpoints

//Add Product
app.MapPost("/products", (CornerStoreDbContext db, Product product) =>
{
    db.Products.Add(product);
    db.SaveChanges();
    return Results.Created($"/products/{product.Id}", product);
});

//Update Product
app.MapPut("/products/{id}", (CornerStoreDbContext db, int id, Product product) =>
{
    Product ProductToUpdate = db.Products.SingleOrDefault(p => p.Id == id);
    if (ProductToUpdate == null)
    {
        return Results.NotFound();
    }
    ProductToUpdate.ProductName = product.ProductName;
    ProductToUpdate.Price = product.Price;
    ProductToUpdate.Brand = product.Brand;
    ProductToUpdate.CategoryId = product.CategoryId;
    db.SaveChanges();
    return Results.NoContent();
});

//get products (including search)
app.MapGet("/products", (CornerStoreDbContext db, string? search) =>
{
    var SearchTerm = search?.Trim().ToLower();

    return Results.Ok(db.Products
    .Where(p => string.IsNullOrEmpty(SearchTerm) || p.ProductName.ToLower().Contains(SearchTerm) || p.Category.CategoryName.ToLower().Contains(SearchTerm))
    .Select(p => new Product
    {
       Id = p.Id,
       ProductName = p.ProductName,
       Price = p.Price,
       Brand = p.Brand,
       CategoryId = p.CategoryId,
       Category = new Category
       {
           Id = p.Category.Id,
           CategoryName = p.Category.CategoryName
       } 
    }).ToList());
});

//Order Endpoints

//Get Order by Id
app.MapGet("/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    Order? order = db.Orders
    .Where(o => o.Id == id)
    .Select(o => new Order
    {
       Id = o.Id,
            CashierId = o.CashierId,
            OrderDate = o.OrderDate,
            PaidOnDate = o.PaidOnDate,
            OrderProducts = o.OrderProducts.Select(op => new OrderProduct
            {
                OrderId = op.OrderId,
                ProductId = op.ProductId,
                Quantity = op.Quantity,
                Product = new Product
                {
                    Id = op.Product.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    CategoryId = op.Product.CategoryId
                }
            }).ToList()   
    }).SingleOrDefault();

    return order is null ? Results.NotFound() : Results.Ok(order);
});

//Get All Orders (with search)
app.MapGet("/orders", (CornerStoreDbContext db, DateTime? orderDate) =>
{
    var start = orderDate?.Date;
    var end = start?.AddDays(1);

    return Results.Ok(db.Orders
    .Where(o => start == null || (o.PaidOnDate >= start && o.PaidOnDate < end))
    .OrderBy(o => o.PaidOnDate)
    .Select(o => new Order
    {
        Id = o.Id,
        CashierId = o.CashierId,
        OrderDate = o.OrderDate,
        PaidOnDate = o.PaidOnDate,
        Cashier = new Cashier
        {
            Id = o.Cashier.Id,
            FirstName = o.Cashier.FirstName,
            LastName = o.Cashier.LastName
        },
        OrderProducts = o.OrderProducts.Select(po => new OrderProduct
        {
            ProductId = po.ProductId,
            Quantity = po.Quantity,
            Product = new Product
            {
                Id = po.Product.Id,
                ProductName = po.Product.ProductName,
                Price = po.Product.Price,
                Brand = po.Product.Brand,
                CategoryId = po.Product.CategoryId
            }
        }).ToList()
    }).ToList());
});

//Delete an Order 
app.MapDelete("/orders/{id}", (CornerStoreDbContext db, int id) =>
{
    Order order = db.Orders.Include(o => o.OrderProducts).SingleOrDefault(o => o.Id == id);
    if (order == null)
    {
        return Results.NotFound();
    }
    db.Orders.Remove(order);
    db.SaveChanges();
    return Results.NoContent();
});

//Create an Order (with products)
app.MapPost("/orders", (CornerStoreDbContext db, Order order) =>
{
    try
    {
        order.OrderDate = DateTime.Now;
        db.Orders.Add(order);
        db.SaveChanges();

        Order placement = db.Orders
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .Single(o => o.Id == order.Id);

        return Results.Created($"/orders/{order.Id}", order);
    }
    catch(DbUpdateException)
    {
        return Results.BadRequest("Invalid Data submitted");
    }
});

app.Run();

//don't move or change this!
public partial class Program { }