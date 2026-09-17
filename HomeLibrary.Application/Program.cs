using HomeLibrary.Application.Endpoints;
using HomeLibrary.Application.Services;
using HomeLibrary.Domain.Repositories;
using HomeLibrary.Domain.Services;
using HomeLibrary.Infrastructure.Data;
using HomeLibrary.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);


var dbSettings = builder.Configuration.GetSection("DbSettings");
var dbProvider = dbSettings["DbProvider"];

string connectionString = dbProvider switch
{
    "SqlServer" => dbSettings.GetSection("ConnectionStrings")["SqlServer"]!,
    "LocalDb" => dbSettings.GetSection("ConnectionStrings")["LocalDb"]!,
    "PostgreSQL" => dbSettings.GetSection("ConnectionStrings")["PostgreSQL"]!,
    _ => throw new Exception($"Неподдерживаемый провайдер базы данных: {dbProvider}")
};

builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (dbProvider)
    {
        case "SqlServer":
        case "LocalDb":
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly("HomeLibrary.Infrastructure"));
            break;

        case "PostgreSQL":
            throw new NotImplementedException("Провайдер PostgreSQL подготовлен архитектурно, но пакет Npgsql еще не установлен.");
    }
});


builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IXmlAnalyticsService, XmlAnalyticsService>();

// CORS для локальной разработки
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HomeLibrary Minimal API", Version = "v1" });
});


var app = builder.Build();

// Активируем CORS (обязательно перед маппингом эндпоинтов!)
app.UseCors("AllowAll");

//автоматическая миграция
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HomeLibrary API v1");
        c.RoutePrefix = "swagger"; // Интерфейс по адресу /swagger
    });
}

app.MapBookEndpoints();

app.MapGet("/", () => "HomeLibrary Minimal API работает!");

app.Run();
