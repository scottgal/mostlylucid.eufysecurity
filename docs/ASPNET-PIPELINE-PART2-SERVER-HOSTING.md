# Understanding the ASP.NET Core Pipeline - Part 2: Kestrel and The Host (Where Your App Actually Lives)

<!--category-- ASP.NET Core, C#, Kestrel, Hosting -->
<datetime class="hidden">2024-11-08T01:00</datetime>

> **AI GENERATED** - If that offends you, please stop reading.

# Introduction

In Part 1 we covered the middleware pipeline - the stuff YOUR code runs in. But before any of that happens, something has to actually START your application, listen for HTTP requests, and create those `HttpContext` objects we talked about.

That's where Kestrel and the Host come in.

Too many developers just hit F5 and assume "the framework handles it." Then they deploy to production and wonder why their app won't start, or why HTTPS isn't working, or why they're getting terrible performance under load.

Understanding the hosting layer is CRITICAL for production deployments. This is where you configure ports, HTTPS certificates, performance limits, and how your app starts and stops gracefully.

[TOC]

# The Problem

Here's what I see all the time:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Hello World");
app.Run();
```

This works great in development. Then you deploy to production and:
- The app binds to the wrong port
- HTTPS certificate doesn't load
- The app crashes on startup with zero logs
- Performance is terrible because you're hitting default limits
- Graceful shutdown doesn't work and requests get cut off

**DEFAULTS ARE FOR DEVELOPMENT, NOT PRODUCTION**

# What Actually Happens When You Run Your App

When you do `var app = builder.Build(); app.Run();`, here's what ACTUALLY happens:

1. `WebApplication.CreateBuilder(args)` sets up:
   - Configuration system (appsettings.json, env vars, command line)
   - Logging (Console, Debug, EventSource)
   - Dependency injection container
   - Kestrel as the web server
   - Default host settings

2. `builder.Build()` creates:
   - The configured host
   - The middleware pipeline
   - All your registered services

3. `app.Run()`:
   - Starts Kestrel listening on configured ports
   - Begins accepting HTTP connections
   - Blocks until the app shuts down

Simple, right? Except there's a TON of configuration you can (and should) do at each step.

# Kestrel - The Web Server You're Actually Using

Kestrel is ASP.NET Core's built-in, cross-platform web server. It's FAST. Like, really fast. It can handle:
- HTTP/1.1
- HTTP/2
- HTTP/3 (QUIC) in .NET 7+
- TLS/SSL
- WebSockets

## Configuring What Kestrel Listens On

By default in development, Kestrel listens on `http://localhost:5000` and `https://localhost:5001`. In production? WHO KNOWS. It depends on your environment variables and configuration.

HERE'S HOW TO BE EXPLICIT:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // Listen on ALL interfaces on port 8080 (HTTP)
    options.Listen(IPAddress.Any, 8080);

    // Listen on specific IP with HTTPS
    options.Listen(IPAddress.Parse("10.0.0.100"), 8443, listenOptions =>
    {
        listenOptions.UseHttps("mycert.pfx", "password");
    });

    // For containers/cloud, listen on port from environment
    var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "8080");
    options.Listen(IPAddress.Any, port);
});

var app = builder.Build();
```

OR use appsettings.json (my preferred method for production):

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:8080"
      },
      "Https": {
        "Url": "https://*:8443",
        "Certificate": {
          "Path": "/app/certs/certificate.pfx",
          "Password": "your-cert-password"
        }
      }
    }
  }
}
```

*NOTE: Never commit certificate passwords to source control. Use environment variables or Azure Key Vault or similar.*

## HTTPS in Production - Do This Right

HTTPS in production is NON-NEGOTIABLE. Here's how to actually do it:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 443, listenOptions =>
    {
        // Option 1: Load from file
        listenOptions.UseHttps("/app/certs/mycert.pfx", "password");

        // Option 2: Load from certificate store (Windows)
        listenOptions.UseHttps(httpsOptions =>
        {
            httpsOptions.ServerCertificateSelector = (context, host) =>
            {
                // Load cert from Windows certificate store
                using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates
                    .Find(X509FindType.FindBySubjectName, "yourdomain.com", true)
                    .FirstOrDefault();
            };
        });

        // Option 3: Use development certificate (DEV ONLY!)
        listenOptions.UseHttps(); // Loads development cert
    });
});
```

For production I typically use Let's Encrypt with automatic renewal. There are NuGet packages for this:

```bash
dotnet add package LettuceEncrypt
```

```csharp
builder.Services.AddLettuceEncrypt();
// Auto-renews certificates from Let's Encrypt
```

MUCH easier than managing certificates manually.

## Performance Tuning - The Limits Matter

Kestrel has sensible defaults, but "sensible" for development is NOT sensible for production. Here's what I typically configure:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    // Max concurrent connections (default: unlimited)
    // Set this to prevent resource exhaustion under attack
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.MaxConcurrentUpgradedConnections = 100;

    // Max request body size (default: 30MB)
    // If you're accepting file uploads, you'll need to increase this
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB

    // Request timeout (default: 30 seconds for headers)
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);

    // Keep-alive timeout (default: 130 seconds)
    // Lower this if you have lots of idle connections
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(1);

    // Minimum data rate for request/response body
    // Prevents slowloris attacks
    options.Limits.MinRequestBodyDataRate = new MinDataRate(
        bytesPerSecond: 100,
        gracePeriod: TimeSpan.FromSeconds(10)
    );

    // HTTP/2 limits
    options.Limits.Http2.MaxStreamsPerConnection = 100;
    options.Limits.Http2.InitialConnectionWindowSize = 128 * 1024; // 128 KB
});
```

**IMPORTANT**: Set `MaxConcurrentConnections` in production. Without it, a simple DOS attack (open tons of connections) will exhaust your server resources.

## HTTP/2 and HTTP/3

HTTP/2 is enabled by default on HTTPS endpoints. HTTP/3 (QUIC) requires .NET 7+ and explicit configuration:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});
```

*NOTE: HTTP/3 requires QUIC protocol support in your OS and network. It's not always worth the complexity.*

# The Host - Application Lifetime Management

The host is what actually RUNS your application. It manages:
- Dependency injection
- Configuration
- Logging
- Application lifetime (startup/shutdown)

## Configuration - Get This Right

By default, configuration is loaded from:
1. appsettings.json
2. appsettings.{Environment}.json
3. User secrets (dev only)
4. Environment variables
5. Command line arguments

Later sources override earlier ones. This means:

```bash
# Your appsettings.json says PORT=5000
# But you can override it:
dotnet run --Port=8080

# Or with environment variable:
PORT=8080 dotnet run
```

For production, I ALWAYS use environment variables for sensitive config:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add more configuration sources
builder.Configuration
    .AddEnvironmentVariables(prefix: "MYAPP_")
    .AddJsonFile("/config/secrets.json", optional: true)
    .AddAzureKeyVault(/* for production secrets */);

// Access configuration
var connectionString = builder.Configuration.GetConnectionString("Database");
var apiKey = builder.Configuration["MyApp:ApiKey"];
```

## Logging Configuration

Default logging is VERBOSE in development, MINIMAL in production. Configure it:

```csharp
builder.Logging.ClearProviders(); // Remove defaults

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    // Production: structured logging only
    builder.Logging.AddJsonConsole(); // For container log aggregation
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}
```

Better yet, use Serilog (covered in my logging post).

## Graceful Shutdown - Don't Cut Off Requests

When your app shuts down (container restart, deployment, etc.), you want to:
1. Stop accepting NEW requests
2. Finish existing requests
3. Then exit

```csharp
builder.Host.ConfigureHostOptions(options =>
{
    // How long to wait for requests to complete on shutdown
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Hook into lifetime events
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Application has started - warm up caches here");
});

lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Shutdown requested - stop accepting new work");
});

lifetime.ApplicationStopped.Register(() =>
{
    Console.WriteLine("Application stopped - cleanup resources");
});

app.Run();
```

During shutdown, Kestrel:
1. Stops accepting new connections
2. Waits up to `ShutdownTimeout` for existing requests to finish
3. Then forcibly terminates remaining requests

Set `ShutdownTimeout` based on your longest expected request time. For APIs, 30 seconds is usually fine. For background jobs, you might need minutes.

# Running Behind a Reverse Proxy (nginx, IIS, etc.)

In production, Kestrel typically runs BEHIND a reverse proxy. The proxy:
- Handles TLS termination
- Load balancing
- Static file caching
- Request filtering

But this means Kestrel sees the proxy's IP, not the client's IP. You need Forwarded Headers middleware:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // If you KNOW your proxy IPs, add them for security
    options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));

    // In containers (Kubernetes, Docker), you often don't know the proxy IP
    // So clear the lists (LESS secure, only do in trusted environments)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// MUST be before other middleware
app.UseForwardedHeaders();

app.UseHttpsRedirection(); // Now knows if original request was HTTPS
app.UseAuthentication();   // Now has correct client IP
```

**CRITICAL**: Put `UseForwardedHeaders()` BEFORE other middleware, especially authentication. Otherwise your logs/security will have wrong IP addresses.

# Production Hosting Scenarios

## Docker/Containers

```csharp
// In containers, bind to all interfaces
builder.WebHost.ConfigureKestrel(options =>
{
    var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "8080");
    options.Listen(IPAddress.Any, port);
});

// Use JSON console logging for log aggregation
builder.Logging.AddJsonConsole();
```

## Windows Service

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService(); // Enables Windows Service lifetime

// Set content root to app directory, not system32
builder.Host.UseContentRoot(AppContext.BaseDirectory);

var app = builder.Build();
```

## Linux systemd

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSystemd(); // Enables systemd lifetime

var app = builder.Build();
```

Both Windows Service and systemd modes hook into OS lifecycle events for proper startup/shutdown.

# Common Production Mistakes

## Mistake 1: Not Setting MaxRequestBodySize

Default is 30MB. First time someone uploads a 50MB file, your app returns 413 Payload Too Large and you're confused why.

```csharp
options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // Set it explicitly
```

## Mistake 2: Not Configuring HTTPS Certificate Path

Works in dev (uses development certificate). Crashes in production (no certificate found).

ALWAYS configure certificate location explicitly for production.

## Mistake 3: Not Using Forwarded Headers

Your logs show every request coming from `127.0.0.1` (the proxy). Security features based on IP address don't work. Rate limiting doesn't work.

USE FORWARDED HEADERS MIDDLEWARE.

## Mistake 4: Binding to localhost in Containers

```csharp
options.Listen(IPAddress.Loopback, 8080); // WRONG in containers
```

Containers can't reach localhost. Use `IPAddress.Any`:

```csharp
options.Listen(IPAddress.Any, 8080); // CORRECT
```

# In Conclusion

The hosting layer is where your application ACTUALLY lives. Get it wrong and nothing else matters - your app won't start, won't handle load, won't shut down gracefully, or won't be secure.

Key points:
1. **Configure Kestrel explicitly for production** - don't rely on defaults
2. **Set performance limits** - Max connections, request size, timeouts
3. **Handle HTTPS properly** - Use proper certificates, not dev certs
4. **Use Forwarded Headers** - When behind a proxy (which is always in production)
5. **Configure graceful shutdown** - Don't cut off in-flight requests
6. **Environment-specific settings** - Dev and production are DIFFERENT

In Part 3 we'll dive deep into the middleware pipeline - all the stuff that runs BETWEEN Kestrel receiving a request and your endpoint executing.

Now go configure your Kestrel settings before you deploy to production!
