using MostlyLucid.EufySecurity.Demo.HealthChecks;
using MostlyLucid.EufySecurity.Demo.Hubs;
using MostlyLucid.EufySecurity.Demo.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EufySecurity.NET Demo API",
        Version = "v1",
        Description = "Demo ASP.NET Core Web API showcasing EufySecurity.NET functionality",
        Contact = new OpenApiContact
        {
            Name = "EufySecurity.NET",
            Url = new Uri("https://github.com/eufy-security/EufySecurity.NET")
        },
        License = new OpenApiLicense
        {
            Name = "Unlicense",
            Url = new Uri("https://unlicense.org/")
        }
    });

    // Include XML comments if they exist
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add SignalR
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add EufySecurity hosted service
builder.Services.AddSingleton<EufySecurityHostedService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<EufySecurityHostedService>());

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<EufySecurityHealthCheck>("EufySecurity");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EufySecurity.NET Demo API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map Razor Pages
app.MapRazorPages();

// Map SignalR hub
app.MapHub<EufyEventsHub>("/hubs/events");

// Map health checks
app.MapHealthChecks("/health");

// Welcome endpoint - redirect to login
app.MapGet("/", () => Results.Redirect("/Auth/Login"));

app.Logger.LogInformation("EufySecurity.NET Demo API starting...");
app.Logger.LogInformation("Swagger UI available at: https://localhost:{Port}/",
    app.Configuration["ASPNETCORE_HTTPS_PORT"] ?? "5001");
app.Logger.LogInformation("SignalR Hub available at: https://localhost:{Port}/hubs/events",
    app.Configuration["ASPNETCORE_HTTPS_PORT"] ?? "5001");

app.Run();
