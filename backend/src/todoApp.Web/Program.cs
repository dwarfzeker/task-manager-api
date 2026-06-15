using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;

using todoApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using todoApp.DI;
using todoApp.Web.Middleware;
var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
Console.WriteLine($"JWTsettings Secret loaded: {(string.IsNullOrEmpty(jwtSecret) ? "❌ NO" : "✅ YES")}");
if (string.IsNullOrEmpty(jwtSecret))
{
    jwtSecret = builder.Configuration["Jwt:Secret"];
    Console.WriteLine($"JWT Secret loaded: {(string.IsNullOrEmpty(jwtSecret) ? "❌ NO" : "✅ YES")}");
}
Console.WriteLine($"JWT Secret length: {jwtSecret?.Length ?? 0}");
Console.WriteLine($"JWT Issuer: {jwtIssuer ?? "NOT SET"}");
Console.WriteLine($"JWT Audience: {jwtAudience ?? "NOT SET"}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4173",  
                "http://localhost:3000",  
                "http://localhost:1234",
                "http://localhost:8080",
                "http://localhost"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory:
        
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });
    options.AddPolicy("api", context =>
    {
        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous",
        factory:
        _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            TokensPerPeriod = 10,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 5
        
        });
    });
});
builder.Services.AddMemoryCache();
builder.Services.ConfigureServces();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "todos.db");
    options.UseSqlite($"Data Source={dbPath}", b => b.MigrationsAssembly("todoApp.Data"));
});

var dbPath = builder.Configuration.GetConnectionString("DefaultConnection") 
             ?? Path.Combine(builder.Environment.ContentRootPath, "data", "todos.db");

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = builder.Configuration["JwtSettings:Secret"] == null ? 
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])) : 
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])),
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "TodoApp API", 
        Version = "v1" 
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: 'Bearer eyJhbGciOiJIUzI1NiIs...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
        }
    });
});

builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
var app = builder.Build();


foreach (var source in builder.Configuration.Sources)
{
    Console.WriteLine($"Config source: {source.GetType().Name}");
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Применяем миграции, если база не существует или устарела
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
        
        // Для SQLite - создаём базу, если её нет
        if (ex.Message.Contains("no such table"))
        {
            Console.WriteLine("Creating database from scratch...");
            dbContext.Database.EnsureCreated();
            Console.WriteLine("✅ Database created successfully");
        }
    }
}


app.UseResponseCaching();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.MapControllers();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LastActivityMiddleware>();

app.UseSwagger();


app.UseSwaggerUI();



app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }