using CoreBookingPlatform.OrderService.Data.Context;
using CoreBookingPlatform.OrderService.Mappings;
using CoreBookingPlatform.OrderService.Services.Implementations;
using CoreBookingPlatform.OrderService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddLogging();


builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProductService:BaseUrl"]!);
});
builder.Services.AddHttpClient("CartService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CartService:BaseUrl"]!);
});
builder.Services.AddHttpClient("AdapterService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AdapterService:BaseUrl"]!);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
