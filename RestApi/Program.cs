using Microsoft.EntityFrameworkCore;
using RestApi.Data;

var builder = WebApplication.CreateBuilder(args);

// === MySQL avec Pomelo ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=productsdb;User=root;Password=root123;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mysql =>
        {
            mysql.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            mysql.CommandTimeout(60);
        }));

// === Controllers + Swagger ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Products API", Version = "v1", Description = "API REST .NET 8 + EF Core + MySQL" });
});

// === CORS (pour tests frontend) ===
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// === Health Checks ===
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("mysql");

var app = builder.Build();

// === Auto-migration au démarrage ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retries = 0;
    while (retries < 20)
    {
        try
        {
            app.Logger.LogInformation("Tentative de connexion MySQL ({attempt}/20)...", retries + 1);
            db.Database.EnsureCreated();
            app.Logger.LogInformation("✅ Base MySQL créée avec succès !");
            break;
        }
        catch (Exception ex)
        {
            retries++;
            app.Logger.LogWarning("⏳ MySQL pas encore prêt ({attempt}/20): {msg}", retries, ex.Message);
            if (retries >= 20) throw;
            Thread.Sleep(3000);
        }
    }
}

// === Middleware ===
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Products API v1"));
app.MapHealthChecks("/healthz");
app.MapControllers();

// === Root redirect to Swagger ===
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
