using Microsoft.EntityFrameworkCore;
using ServiceConnect.BLL.Data;
using ServiceConnect.BLL.Factories;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();

// Build connection string from environment variables
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "ServiceConnect";
var integratedSecurity = Environment.GetEnvironmentVariable("DB_INTEGRATED_SECURITY")?.ToLower() == "true";

string connectionString;
if (integratedSecurity)
{
    connectionString = $"Server={dbServer};Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
}
else
{
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    connectionString = $"Server={dbServer};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
}

// Add connection string to configuration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["ConnectionStrings:ServiceConnectDB"] = connectionString
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ServiceConnect API",
        Version = "v1",
        Description = "Phase 3: Application with dual BLL implementations (LINQ/EF and Stored Procedures)"
    });
});

// Configure DbContext for Entity Framework
builder.Services.AddDbContext<ServiceConnectDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register ServiceFactory as a scoped service
// The BLL type (LinqEF or StoredProcedure) can be switched via query parameter or config
builder.Services.AddScoped<ServiceFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var dbContext = sp.GetRequiredService<ServiceConnectDbContext>();

    // Read BLL type from environment variable (default to LINQ/EF)
    var bllTypeSetting = Environment.GetEnvironmentVariable("BLL_TYPE") ?? "LinqEF";
    var bllType = bllTypeSetting.ToLower() == "storedprocedure" ? BllType.StoredProcedure : BllType.LinqEF;

    return new ServiceFactory(config, bllType, dbContext);
});

// Add CORS policy for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNextJs");

// Avoid redirecting to HTTPS in Development to keep local http://localhost:5000 working smoothly
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// Custom endpoint to switch BLL type at runtime
app.MapGet("/api/config/bll-type", () =>
{
    var currentType = Environment.GetEnvironmentVariable("BLL_TYPE") ?? "LinqEF";
    return Results.Ok(new { bllType = currentType });
});

app.MapPost("/api/config/bll-type/{type}", (string type) =>
{
    if (type.ToLower() != "linqef" && type.ToLower() != "storedprocedure")
    {
        return Results.BadRequest(new { error = "Invalid BLL type. Use 'LinqEF' or 'StoredProcedure'" });
    }

    Environment.SetEnvironmentVariable("BLL_TYPE", type);
    return Results.Ok(new { message = $"BLL type switched to {type}", bllType = type });
});

Console.WriteLine("==========================================================");
Console.WriteLine("ServiceConnect API - Phase 3");
Console.WriteLine("==========================================================");
Console.WriteLine($"Database: {dbName} on {dbServer}");
Console.WriteLine($"BLL Type: {Environment.GetEnvironmentVariable("BLL_TYPE") ?? "LinqEF (default)"}");
Console.WriteLine($"Swagger UI: https://localhost:5001/swagger");
Console.WriteLine("==========================================================");

app.Run();
