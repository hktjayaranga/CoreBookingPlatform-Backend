using CoreBookingPlatform.AdapterService.Adapters.AbcAdapter;
using CoreBookingPlatform.AdapterService.Adapters.CdeAdapter;
using CoreBookingPlatform.AdapterService.Interfaces;
using CoreBookingPlatform.AdapterService.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Adapter Service API",
        Version = "v1",
        Description = "API for triggering product and content imports via adapters"
    });
});

//builder.Services.AddAdapterServices();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAdapter, AbcAdapter>();
builder.Services.AddScoped<IAdapter, CdeAdapter>();
builder.Services.AddHttpClient("AbcApi", c => {
    c.BaseAddress = new Uri(builder.Configuration["ExternalApis:Abc:BaseUrl"]!.TrimEnd('/') + "/");
});
builder.Services.AddHttpClient("CdeApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:Cde:BaseUrl"]!.TrimEnd('/') + "/");
});
builder.Services.AddHttpClient("ProductApi", c => {
    c.BaseAddress = new Uri(builder.Configuration["ProductService:BaseUrl"]!.TrimEnd('/') + "/");
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddHostedService<StartupImportService>();

//builder.Services.AddScoped<IAdapterService, AdapterService>();
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
