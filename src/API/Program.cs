using Application;
using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using Models.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<_2019sbdContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/devices", (IDeviceService service) => Results.Ok(service.GetAll()));

app.MapGet("/api/devices/{id:int}", (int id, IDeviceService service) =>
{
    var result = service.GetById(id);
    return result == null ? Results.NotFound() : Results.Ok(result);
});

app.MapPost("/api/devices", async (DeviceCreateRequest device, IDeviceService service) =>
{
    var created = service.Create(device);
    return Results.Created($"/api/devices/{created.Id}", created);
});

app.MapPut("/api/devices/{id:int}", async (int id, DeviceCreateRequest device, IDeviceService service) =>
{
    var updated = service.Update(id, device);
    return updated == null ? Results.NotFound() : Results.Ok(updated);
});

app.MapDelete("/api/devices/{id:int}", (int id, IDeviceService service) =>
{
    return service.Delete(id) ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/api/employees", (IEmployeeService service) => Results.Ok(service.GetAll()));

app.MapGet("/api/employees/{id:int}", (int id, IEmployeeService service) =>
{
    var result = service.GetById(id);
    return result == null ? Results.NotFound() : Results.Ok(result);
});

app.Run("http://localhost:5300");