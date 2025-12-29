using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TechAssistPro.Infrastructure.Observability;

namespace TechAssistPro.Ticketing.DependencyInjection
{
    public static class ObservabilityExtensions
    {
        public static WebApplicationBuilder AddLogger(
          this WebApplicationBuilder builder)
        {
            var otel = builder.Configuration.GetSection("OpenTelemetry");
            string? lokiUrl = otel["Loki:Url"] ?? "http://localhost:3100";
            string? appName = otel["ServiceName"] ?? "TechAssistPro.Ticketing";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", appName)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.GrafanaLoki(
                    lokiUrl,
                    labels: new[]
                    {
                    new LokiLabel { Key = "app", Value = appName.ToLowerInvariant().Replace(" ", "-") },
                    new LokiLabel { Key = "environment", Value = builder.Environment.EnvironmentName.ToLowerInvariant() }
                    })
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }


        public static WebApplication UseRequestLogging(this WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                    diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);

                    if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
                    {
                      diagnosticContext.Set("CorrelationId", correlationId.ToString());
                    }
                };

                options.MessageTemplate =
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            });

            return app;
        }


        public static WebApplicationBuilder AddTracing(
       this WebApplicationBuilder builder)
        {
            var otel = builder.Configuration.GetSection("OpenTelemetry");
            string? serviceName = otel["ServiceName"] ?? "TechAssistPro.Ticketing";
            string[]? activitySources = new[] { serviceName };

            var otlpEndpoint = otel["Otlp:Endpoint"]
                ?? "";

            var activitySource = new ActivitySource(serviceName);
            builder.Services.AddSingleton(activitySource);

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(serviceName)
                            .AddAttributes(new Dictionary<string, object>
                            {
                                ["environment"] = builder.Environment.EnvironmentName,

                            }))
                        .AddSource(activitySources)
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.Filter = httpContext =>
                        {
                            var path = httpContext.Request.Path.Value ?? string.Empty;
                            return !path.StartsWith("/health") && !path.StartsWith("/swagger");
                        };
                            options.EnrichWithHttpRequest = (activity, request) =>
                            {
                                activity.SetTag("http.request_content_length", request.ContentLength);
                                activity.SetTag("http.request_content_type", request.ContentType);

                                if (request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
                                {
                                  activity.SetTag("correlation.id", correlationId.ToString());
                                }

                            };
                            options.EnrichWithHttpResponse = (activity, response) =>
                            {
                                activity.SetTag("http.response_content_length", response.ContentLength);
                                activity.SetTag("http.response_content_type", response.ContentType);
                            };
                        })
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithHttpRequestMessage = (activity, request) =>
                            {
                                activity.SetTag("http.request.method", request.Method.ToString());
                            };
                            options.EnrichWithHttpResponseMessage = (activity, response) =>
                            {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                            };
                        })
                        .AddEntityFrameworkCoreInstrumentation(options =>
                            {
                                if (otel.GetValue<bool>("Database:CaptureSql"))
                                {
                                    options.EnrichWithIDbCommand = (activity, command) =>
                                    {
                                        if (activity == null || command == null)
                                            return;

                                        activity.SetTag("db.system", "postgresql");
                                        activity.SetTag("db.statement", command.CommandText);
                                        activity.SetTag("correlation.id", CorrelationContext.CorrelationId);
                                    };
                                }
                            })
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        });

                    if (builder.Environment.IsDevelopment())
                    {
                        tracerProviderBuilder.AddConsoleExporter();
                    }
                });

            return builder;
        }


    }
}