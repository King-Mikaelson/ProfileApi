using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProfileApi.Services;
using StringAnalyzer.Data;
using StringAnalyzer.Exceptions;
using StringAnalyzer.Models;
using StringAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<ProfileOptions>(builder.Configuration.GetSection("Profile"));

// Add EF Core with SQL Server connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Disable automatic 400 responses from model binding
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<StringAnalyzer.Middleware.ValidationFilter>();
});

// Register String Analyzer Service
builder.Services.AddScoped<IStringAnalyzerService, StringAnalyzerService>();
builder.Services.AddControllers();

builder.Services.AddHttpClient<CatFactService>(client =>
{
    // no base address; we call absolute URL in the service
    client.Timeout = TimeSpan.FromSeconds(5); // set reasonable timeout
});

builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter("default", options =>
{
    options.PermitLimit = 10;
    options.Window = TimeSpan.FromSeconds(10);
    options.QueueLimit = 0;
}));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseMiddleware<StringAnalyzer.Middleware.Middleware>();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();


app.MapControllers();

// Everything above stays the same
Console.WriteLine("Current environment: " + builder.Environment.EnvironmentName);
Console.WriteLine("Connection string:");
Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));

app.Run();


