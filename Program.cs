using Microsoft.AspNetCore.RateLimiting;
using ProfileApi.Models;
using ProfileApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<ProfileOptions>(builder.Configuration.GetSection("Profile"));

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

app.Run();
