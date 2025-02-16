using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizAPI.Data;
using QuizAPI.Services;
using QuizAPI.Controllers;
using QuizAPI.Routes;
using QuizAPI.Filters;
using QuizAPI.Utils;
using QuizAPI.Middleware;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

builder.Services.AddScoped<UserController>();
builder.Services.AddScoped<QuizController>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["FrontendURL"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add custom services
builder.Services.AddScoped<EmailConfig>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuthFilter>();
builder.Services.AddScoped<TokenHelper>(provider =>
    new TokenHelper(builder.Configuration["Jwt:Secret"])); // Use JWT secret from appsettings.json

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();
app.UseAuthorization();

// AuthMiddleware should come AFTER UseAuthorization for proper token validation
//app.UseMiddleware<AuthMiddleware>();

app.MapControllers(); // Add this line to enable controller routing

app.UseMiddleware<ErrorMiddleware>();

//app.MapUserRoutes();

app.Run();