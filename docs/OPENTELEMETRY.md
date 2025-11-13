# OpenTelemetry Integration Guide

**Comprehensive guide for monitoring MostlyLucid.EufySecurity with Grafana, Prometheus, and OpenTelemetry**

## Table of Contents
- [Overview](#overview)
- [Quick Start](#quick-start)
- [Available Metrics](#available-metrics)
- [Distributed Tracing](#distributed-tracing)
- [Grafana Setup](#grafana-setup)
- [Prometheus Configuration](#prometheus-configuration)
- [Example Dashboards](#example-dashboards)
- [Best Practices](#best-practices)

## Overview

MostlyLucid.EufySecurity includes comprehensive OpenTelemetry instrumentation for:
- **Distributed Tracing** - Track operations across your application
- **Metrics** - Monitor performance, errors, and business metrics
- **Logs** - Structured logging with correlation IDs (via ILogger)

### Supported Backends
- ✅ **Grafana** - Visualization and dashboarding
- ✅ **Prometheus** - Metrics storage and querying
- ✅ **Tempo/Jaeger** - Distributed tracing backend
- ✅ **OpenTelemetry Protocol (OTLP)** - Standard telemetry protocol

## Quick Start

### 1. Add NuGet Packages

For your consuming application:

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
```

### 2. Configure in Program.cs

```csharp
using MostlyLucid.EufySecurity.Telemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("YourServiceName", serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddSource(EufySecurityInstrumentation.ActivitySource.Name)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())  // For Grafana Tempo/Jaeger
    .WithMetrics(metrics => metrics
        .AddMeter(EufySecurityInstrumentation.Meter.Name)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter());  // For Prometheus

var app = builder.Build();

// Map Prometheus scraping endpoint
app.MapPrometheusScrapingEndpoint();  // Available at /metrics

app.Run();
```

### 3. Test Your Setup

Start your application and verify metrics are exposed:

```bash
curl http://localhost:5000/metrics
```

You should see Prometheus-format metrics output.

## Available Metrics

### Counters

#### `eufy.authentication.attempts`
Number of authentication attempts.
- **Unit:** `{attempt}`
- **Tags:**
  - `result`: `success`, `failure`, `2fa_required`
  - `requires_2fa`: `true`, `false`

**Example PromQL:**
```promql
rate(eufy_authentication_attempts_total[5m])
sum by (result) (eufy_authentication_attempts_total)
```

#### `eufy.authentication.failures`
Number of authentication failures.
- **Unit:** `{failure}`
- **Tags:**
  - `reason`: `invalid_credentials`, `network_error`, `2fa_failed`, `exception`, `unknown_error`

**Example PromQL:**
```promql
sum by (reason) (eufy_authentication_failures_total)
```

#### `eufy.http.api.calls`
Number of HTTP API calls to Eufy cloud.
- **Unit:** `{call}`
- **Tags:**
  - `endpoint`: `get_stations`, `get_devices`, `login`
  - `method`: `GET`, `POST`
  - `status_code`: `200`, `401`, `500`, etc.

**Example PromQL:**
```promql
rate(eufy_http_api_calls_total[5m])
sum by (endpoint) (eufy_http_api_calls_total)
```

#### `eufy.http.api.errors`
Number of HTTP API errors.
- **Unit:** `{error}`
- **Tags:**
  - `endpoint`: API endpoint name
  - `error_type`: Exception type name

#### `eufy.p2p.connection.attempts`
Number of P2P connection attempts.
- **Unit:** `{attempt}`
- **Tags:**
  - `connection_type`: `local`, `remote`, `quickest`
  - `result`: `success`, `failure`

#### `eufy.livestream.sessions`
Number of livestream sessions.
- **Unit:** `{session}`
- **Tags:**
  - `device_type`: Device type enum
  - `result`: `started`, `stopped`, `error`

#### `eufy.livestream.bytes_received`
Total bytes received from livestreams.
- **Unit:** `By` (bytes)
- **Tags:**
  - `data_type`: `video`, `audio`
  - `device_serial`: Device serial number

#### `eufy.push.notifications_received`
Number of push notifications received.
- **Unit:** `{notification}`
- **Tags:**
  - `notification_type`: Notification type
  - `device_type`: Device type

#### `eufy.devices.refreshes`
Number of device list refreshes.
- **Unit:** `{refresh}`
- **Tags:**
  - `trigger`: `automatic`, `manual`

#### `eufy.errors`
Total errors by type.
- **Unit:** `{error}`
- **Tags:**
  - `error.type`: Exception type name
  - `operation`: Operation name

### Histograms

#### `eufy.http.api.duration`
Duration of HTTP API calls.
- **Unit:** `ms` (milliseconds)
- **Tags:**
  - `endpoint`: API endpoint name
  - `method`: HTTP method
  - `status_code`: HTTP status code

**Example PromQL:**
```promql
histogram_quantile(0.95, rate(eufy_http_api_duration_bucket[5m]))
histogram_quantile(0.99, rate(eufy_http_api_duration_bucket[5m]))
```

#### `eufy.authentication.duration`
Duration of authentication flow.
- **Unit:** `ms`
- **Tags:**
  - `requires_2fa`: `true`, `false`

#### `eufy.p2p.connection.duration`
Duration to establish P2P connections.
- **Unit:** `ms`
- **Tags:**
  - `connection_type`: `local`, `remote`, `quickest`
  - `result`: `success`, `failure`

#### `eufy.devices.refresh.duration`
Duration of device list refresh operations.
- **Unit:** `ms`
- **Tags:**
  - `device_count`: Number of devices

#### `eufy.livestream.frame_processing_time`
Time to process livestream frames.
- **Unit:** `ms`
- **Tags:**
  - `data_type`: `video`, `audio`

### Observable Gauges

#### `eufy.devices.connected`
Number of currently connected devices.
- **Unit:** `{device}`

**Example PromQL:**
```promql
eufy_devices_connected
```

#### `eufy.stations.count`
Number of stations (hubs).
- **Unit:** `{station}`

#### `eufy.livestream.active`
Number of active livestream sessions.
- **Unit:** `{stream}`

#### `eufy.p2p.connections.active`
Number of active P2P connections.
- **Unit:** `{connection}`

#### `eufy.authentication.status`
Current authentication status.
- **Unit:** `{status}`
- **Values:** `1` = authenticated, `0` = not authenticated

#### `eufy.authentication.token_expiration_seconds`
Seconds until authentication token expires.
- **Unit:** `s` (seconds)

## Distributed Tracing

### Available Spans

#### `eufy.authenticate`
Complete authentication flow including 2FA.
- **Tags:**
  - `requires_2fa`: Boolean
  - `result`: `success`, `2fa_required`, `failure`
- **Events:**
  - Exceptions recorded automatically

#### `eufy.http.get_stations`
Retrieve station list from cloud.
- **Tags:**
  - `http.endpoint`: API endpoint
  - `http.method`: HTTP method
  - `http.status_code`: Response status

#### `eufy.http.get_devices`
Retrieve device list from cloud.
- **Tags:**
  - `http.endpoint`: API endpoint
  - `http.method`: HTTP method
  - `http.status_code`: Response status

#### `eufy.devices.refresh`
Device list refresh operation.
- **Tags:**
  - `station.count`: Number of stations discovered
  - `device.count`: Number of devices discovered

### Trace Visualization

In Grafana/Tempo, traces show:
- Complete request flow
- Operation timing and dependencies
- Exception details
- Tags for filtering and analysis

## Grafana Setup

### Docker Compose Setup

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  # Prometheus - Metrics storage
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'

  # Tempo - Trace storage
  tempo:
    image: grafana/tempo:latest
    ports:
      - "3200:3200"   # Tempo
      - "4317:4317"   # OTLP gRPC
      - "4318:4318"   # OTLP HTTP
    volumes:
      - ./tempo.yml:/etc/tempo.yml
      - tempo-data:/tmp/tempo
    command: ["-config.file=/etc/tempo.yml"]

  # Grafana - Visualization
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_AUTH_ANONYMOUS_ENABLED=true
      - GF_AUTH_ANONYMOUS_ORG_ROLE=Admin
    volumes:
      - grafana-data:/var/lib/grafana
      - ./grafana-datasources.yml:/etc/grafana/provisioning/datasources/datasources.yml

volumes:
  prometheus-data:
  tempo-data:
  grafana-data:
```

### Prometheus Configuration

Create `prometheus.yml`:

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'eufy-security'
    static_configs:
      - targets: ['host.docker.internal:5000']  # Your app
    metrics_path: '/metrics'
```

### Tempo Configuration

Create `tempo.yml`:

```yaml
server:
  http_listen_port: 3200

distributor:
  receivers:
    otlp:
      protocols:
        grpc:
        http:

storage:
  trace:
    backend: local
    local:
      path: /tmp/tempo/traces

query_frontend:
  search:
    enabled: true
```

### Grafana Data Sources

Create `grafana-datasources.yml`:

```yaml
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true

  - name: Tempo
    type: tempo
    access: proxy
    url: http://tempo:3200
    jsonData:
      tracesToLogs:
        datasourceUid: 'loki'  # Optional: if you have Loki
      serviceMap:
        datasourceUid: 'prometheus'
```

### Start the Stack

```bash
docker-compose up -d
```

Access Grafana at http://localhost:3000

## Example Dashboards

### Dashboard 1: Authentication Monitoring

```json
{
  "dashboard": {
    "title": "Eufy Security - Authentication",
    "panels": [
      {
        "title": "Authentication Attempts",
        "targets": [
          {
            "expr": "sum by (result) (rate(eufy_authentication_attempts_total[5m]))"
          }
        ]
      },
      {
        "title": "Authentication Success Rate",
        "targets": [
          {
            "expr": "sum(rate(eufy_authentication_attempts_total{result=\"success\"}[5m])) / sum(rate(eufy_authentication_attempts_total[5m])) * 100"
          }
        ]
      },
      {
        "title": "Authentication Duration (p95)",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(eufy_authentication_duration_bucket[5m]))"
          }
        ]
      },
      {
        "title": "Token Expiration Time",
        "targets": [
          {
            "expr": "eufy_authentication_token_expiration_seconds"
          }
        ]
      }
    ]
  }
}
```

### Dashboard 2: Device & Station Monitoring

Key Panels:
- **Connected Devices**: `eufy_devices_connected`
- **Station Count**: `eufy_stations_count`
- **Device Refresh Rate**: `rate(eufy_devices_refreshes_total[5m])`
- **Refresh Duration**: `histogram_quantile(0.95, rate(eufy_devices_refresh_duration_bucket[5m]))`

### Dashboard 3: API Performance

Key Panels:
- **API Call Rate**: `sum by (endpoint) (rate(eufy_http_api_calls_total[5m]))`
- **API Errors**: `sum by (endpoint, error_type) (rate(eufy_http_api_errors_total[5m]))`
- **API Latency (p50, p95, p99)**:
  ```promql
  histogram_quantile(0.50, rate(eufy_http_api_duration_bucket[5m]))
  histogram_quantile(0.95, rate(eufy_http_api_duration_bucket[5m]))
  histogram_quantile(0.99, rate(eufy_http_api_duration_bucket[5m]))
  ```
- **Error Rate**: `sum(rate(eufy_errors_total[5m])) by (error_type, operation)`

### Dashboard 4: Livestream Monitoring

Key Panels:
- **Active Livestreams**: `eufy_livestream_active`
- **Session Start/Stop Rate**: `rate(eufy_livestream_sessions_total[5m]) by (result)`
- **Bytes Received**: `rate(eufy_livestream_bytes_received_total[5m]) by (data_type)`
- **Frame Processing Time**: `histogram_quantile(0.95, rate(eufy_livestream_frame_processing_time_bucket[5m]))`

## Best Practices

### 1. Metric Cardinality

**DO:**
- Use predefined tag values (e.g., `success`, `failure`)
- Limit dynamic tags (e.g., device serial numbers)
- Use histograms for latency measurements

**DON'T:**
- Add user IDs or high-cardinality data as tags
- Create unbounded tag values
- Use counters for gauges (e.g., active connections)

### 2. Alerting Rules

Example Prometheus alerts:

```yaml
groups:
  - name: eufy_security_alerts
    rules:
      - alert: EufyAuthenticationFailureHigh
        expr: rate(eufy_authentication_failures_total[5m]) > 0.1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High authentication failure rate"
          description: "Authentication failures > 0.1/sec for 5 minutes"

      - alert: EufyDevicesDisconnected
        expr: eufy_devices_connected == 0
        for: 10m
        labels:
          severity: critical
        annotations:
          summary: "No devices connected"
          description: "All Eufy devices disconnected for 10 minutes"

      - alert: EufyApiLatencyHigh
        expr: histogram_quantile(0.95, rate(eufy_http_api_duration_bucket[5m])) > 5000
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "High API latency"
          description: "95th percentile API latency > 5 seconds"

      - alert: EufyTokenExpiringSoon
        expr: eufy_authentication_token_expiration_seconds < 3600
        labels:
          severity: warning
        annotations:
          summary: "Authentication token expiring soon"
          description: "Token expires in less than 1 hour"
```

### 3. Sampling for High-Volume Traces

For high-volume applications:

```csharp
.WithTracing(tracing => tracing
    .SetSampler(new TraceIdRatioBasedSampler(0.1))  // Sample 10% of traces
    .AddSource(EufySecurityInstrumentation.ActivitySource.Name)
    // ... other configuration
)
```

### 4. Performance Impact

OpenTelemetry overhead:
- **Metrics**: < 1% CPU overhead
- **Tracing (100% sampling)**: 2-5% overhead
- **Tracing (10% sampling)**: < 1% overhead

## Troubleshooting

### Metrics Not Appearing

1. **Check endpoint**:
   ```bash
   curl http://localhost:5000/metrics
   ```

2. **Verify Prometheus scraping**:
   - Check Prometheus targets: http://localhost:9090/targets
   - Ensure firewall allows access
   - Check scrape interval

3. **Enable debug logging**:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "OpenTelemetry": "Debug"
       }
     }
   }
   ```

### Traces Not Showing in Grafana

1. **Verify OTLP exporter configuration**:
   ```csharp
   .AddOtlpExporter(options =>
   {
       options.Endpoint = new Uri("http://tempo:4317");
   })
   ```

2. **Check Tempo is receiving traces**:
   ```bash
   curl http://localhost:3200/api/search
   ```

3. **Verify service name matches**:
   - Must match between app and Grafana queries

## Advanced Topics

### Custom Metrics

Add your own metrics:

```csharp
using MostlyLucid.EufySecurity.Telemetry;

// In your code
EufySecurityInstrumentation.Meter.CreateCounter<long>(
    "your.custom.metric",
    unit: "{count}",
    description: "Your custom metric description");
```

### Trace Context Propagation

Traces automatically propagate through:
- HTTP calls (via `HttpClient`)
- ASP.NET Core requests
- SignalR connections (with additional config)

### Exemplars

Link metrics to traces in Grafana:

```csharp
.WithMetrics(metrics => metrics
    .AddMeter(EufySecurityInstrumentation.Meter.Name)
    .AddPrometheusExporter(options =>
    {
        options.EnableOpenMetrics = true;  // Required for exemplars
    }))
```

## Resources

- [OpenTelemetry .NET Docs](https://opentelemetry.io/docs/instrumentation/net/)
- [Prometheus Query Language](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [Grafana Dashboards](https://grafana.com/grafana/dashboards/)
- [Tempo Documentation](https://grafana.com/docs/tempo/latest/)

---

**Need Help?** Open an issue on GitHub or check the documentation index.
