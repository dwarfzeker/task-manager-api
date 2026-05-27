using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using todoApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using todoApp.DI;
using todoApp.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();


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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //dbContext.Database.EnsureDeleted();  // Удаляет старую БД
   // dbContext.Database.EnsureCreated(); 
    dbContext.Database.Migrate();        // Создаёт новую с миграциями
    Console.WriteLine("Database migrated successfully!");
}
foreach (var source in builder.Configuration.Sources)
{
    Console.WriteLine($"Config source: {source.GetType().Name}");
}

var secret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(secret))
{
    Console.WriteLine("❌ JWT Secret not found! Check user secrets.");
    Console.WriteLine($"UserSecretsId: {builder.Configuration.GetValue<string>("UserSecretsId")}");
}
else
{
    Console.WriteLine($"✅ JWT Secret loaded (length: {secret.Length})");
    Console.WriteLine($"First 10 chars: {secret[..10]}...");
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
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
}

app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.Run();


public partial class Program { }