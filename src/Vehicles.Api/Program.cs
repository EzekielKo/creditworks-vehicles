using Microsoft.EntityFrameworkCore;
using Vehicles.Api;
using Vehicles.Application;
using Vehicles.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VehiclesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<ManufacturerService>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// Allow the React dev server to call the API during development.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.MapControllers();

app.Run();

// Needed so integration tests can reference the entry point.
public partial class Program;
