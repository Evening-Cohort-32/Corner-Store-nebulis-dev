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
app.MapPost("/api/cashier", (CornerStoreDbContext db, Cashier cashier) =>
{
    db.Cashiers.Add(cashier);
    db.SaveChanges();
    return Results.Created($"/api/cashier/{cashier.Id}", cashier);
});

//Get a cashier by Id
app.MapGet("/api/cashier/{id}", (CornerStoreDbContext db, int id) =>
{
    return db.Cashiers
    .Select(c => new Cashier
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Orders = c.Orders.Select(o => new Order
        {
            Id = o.Id,
            PaidOnDate = o.PaidOnDate
        }).ToList()
    });
});

//Product Endpoints

//Add Product
app.MapPost("/api/products", (CornerStoreDbContext db, Product product) =>
{
    db.Products.Add(product);
    db.SaveChanges();
    return Results.Created($"/api/product/{product.Id}", product);
});

//Update Product
app.MapPut("/api/products/{id}", (CornerStoreDbContext db, int id, Product product) =>
{
    Product ProductToUpdate = db.Products.SingleOrDefault(p => p.Id == id);
    if (ProductToUpdate == null)
    {
        return Results.NotFound();
    }
    ProductToUpdate.Name = product.Name;
    ProductToUpdate.Price = product.Price;
    ProductToUpdate.Brand = product.Brand;
    ProductToUpdate.CategoryId = product.CategoryId;
    db.SaveChanges();
    return Results.NoContent();
});

//Get all products
app.MapGet("/api/products", (CornerStoreDbContext db) =>
{
    return db.Products
    .Select(p => new Product
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        Brand = p.Brand,
        CategoryId = p.CategoryId,
        Category = new Category
        {
            Id = p.Category.Id,
            Name = p.Category.Name
        }
    })
    .ToList();
});

//get all products by search
app.MapGet("/api/products/search", (CornerStoreDbContext db, string search) =>
{
    if (string.IsNullOrWhiteSpace(search))
    {
        return Results.BadRequest("Please enter a search");
    }

    return Results.Ok(db.Products
    .Where(p => p.Name.ToLower().Contains(search.ToLower()) || p.Category.Name.ToLower().Contains(search.ToLower()))
    .Select(p => new Product
    {
       Id = p.Id,
       Name = p.Name,
       Price = p.Price,
       Brand = p.Brand,
       CategoryId = p.CategoryId,
       Category = new Category
       {
           Id = p.Category.Id,
           Name = p.Category.Name
       } 
    }).ToList());
});

app.Run();

//don't move or change this!
public partial class Program { }