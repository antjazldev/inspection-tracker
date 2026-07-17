using System.Text;
using InspectionTracker.Application.Interfaces;
using InspectionTracker.Application.Services;
using InspectionTracker.Infrastructure.Auth;
using InspectionTracker.Infrastructure.Persistence;
using InspectionTracker.Infrastructure.Repositories;
using InspectionTracker.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Repositories (Infrastructure fulfills Application's contracts)
builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Business services
builder.Services.AddScoped<InspectionService>();
builder.Services.AddScoped<AuthService>();

// JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for the Angular dev server
builder.Services.AddCors(options =>
    options.AddPolicy("client", p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("client");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
