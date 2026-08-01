using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MesaSitec.Aplicacion.Auth;
using MesaSitec.Aplicacion.Sla;
using MesaSitec.Infraestructura;
using MesaSitec.Infraestructura.Auth;
using MesaSitec.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:Default"]
    ?? "Data Source=mesasitec.db";

builder.Services.AddDbContext<MesaSitecDbContext>(opt =>
    opt.UseSqlite(connectionString));

var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET no está configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<ISlaCalculator, SlaCalculator>();

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MesaSitec API", Version = "v1" });

    var esquemaBearer = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token con el formato: Bearer {tu token}"
    };
    opt.AddSecurityDefinition("Bearer", esquemaBearer);
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { esquemaBearer, new List<string>() }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesaSitecDbContext>();
    db.Database.Migrate();

    var fechaBaseTexto = builder.Configuration["SEED_FECHA_BASE"]
        ?? Environment.GetEnvironmentVariable("SEED_FECHA_BASE")
        ?? "2026-01-15T08:00:00Z";

    var fechaBase = DateTime.Parse(fechaBaseTexto).ToUniversalTime();
    DbSeeder.Sembrar(db, fechaBase);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
