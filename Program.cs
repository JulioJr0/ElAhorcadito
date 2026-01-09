using ElAhorcadito.Helpers;
using ElAhorcadito.Mappings;
using ElAhorcadito.Models.Validators;
using ElAhorcadito.Repositories;
using ElAhorcadito.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ElAhorcadito.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Configuración de JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x => x.TokenValidationParameters =
    new TokenValidationParameters
    {
        ClockSkew = TimeSpan.Zero,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? ""))
    });

// Conexión a BD
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AhorcaditoContext>(x =>
    x.UseMySql(cs, ServerVersion.AutoDetect(cs)));

//AutoMapper
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<MappingProfile>();
});

// Repositories
builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IJuegoService, JuegoService>();
builder.Services.AddTransient<ITemaService, TemaService>();
builder.Services.AddTransient<IRachaService, RachaService>();
builder.Services.AddTransient<IGeminiService, GeminiService>();
builder.Services.AddTransient<IPushNotificationService, PushNotificationService>();

// AGREGAR: Background Task Queue
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

// Helpers
builder.Services.AddTransient<JwtHelper>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<RegistroDTOValidator>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

//login 
var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("login.html");
//options.DefaultFileNames.Add("index.html");
app.Use(async (context, next) =>
{
    // No cachear el service worker
    if (context.Request.Path.StartsWithSegments("/sw.js"))
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        context.Response.Headers.Append("Pragma", "no-cache");
        context.Response.Headers.Append("Expires", "0");
    }
    await next();
});
app.UseDefaultFiles(options);
app.UseStaticFiles();

app.UseFileServer();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
//actual