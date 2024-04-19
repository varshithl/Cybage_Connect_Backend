using Cybage_Connect.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var origindetails = "allowallorigins";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(name: origindetails, policy =>
    {
        policy.WithOrigins("*");
        policy.WithHeaders("*");
        policy.WithMethods("*");
    });
});
builder.Services.AddScoped<JWTServices>();
builder.Services.AddDbContext<CybageConnectDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Data Source=GVC1390\\MSSQLSERVER2019;Initial Catalog=CybageConnectDB;User ID=sa;Password=cybage@123456;Encrypt=False"));
});


// Add services to the container.
builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Set to false for local development
            ValidateAudience = true, // Set to false for local development
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["jwt:Issuer"],
            ValidAudience = builder.Configuration["jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])) // Use secret key from configuration
        };
    });


builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ASP.NET 8 Web API",
        Description = "Authentication with JWT"
    });

    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },Array.Empty<string>()
        }
    });

});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(origindetails);
app.UseAuthorization();

app.MapControllers();

app.Run();
