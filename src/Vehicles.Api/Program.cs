using Microsoft.EntityFrameworkCore;
using Vehicles.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<VehiclesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
