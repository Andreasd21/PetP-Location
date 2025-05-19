using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using PetP_Location.Model;
using PetP_Location.Service;

var appBuilder = WebApplication.CreateBuilder(args);

// Build a resource configuration action to set service information.
Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: appBuilder.Configuration.GetValue("ServiceName", defaultValue: "otel-test")!,
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
    serviceInstanceId: Environment.MachineName);

// Configure OpenTelemetry tracing & metrics with auto-start using the
// AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
appBuilder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(builder =>
    {
        builder
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();

        // Use IConfiguration binding for AspNetCore instrumentation options.
        appBuilder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(
            appBuilder.Configuration.GetSection("AspNetCoreInstrumentation"));

        builder.AddOtlpExporter(otlpOptions =>
        {
            // Use IConfiguration directly for Otlp exporter endpoint option.
            otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue(
                  "Otlp:Endpoint", defaultValue: "http://localhost:4317")!);
        });
    })
    .WithMetrics(builder =>
    {
        builder
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();

        builder.AddOtlpExporter(otlpOptions =>
        {
            // Use IConfiguration directly for Otlp exporter endpoint option.
            otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue(
                  "Otlp:Endpoint", defaultValue: "http://localhost:4317")!);
        });
    });

// Clear default logging providers used by WebApplication host.
appBuilder.Logging.ClearProviders();

// Configure OpenTelemetry Logging.
appBuilder.Logging.AddOpenTelemetry(options =>
{
    // Note: See appsettings.json Logging:OpenTelemetry section for configuration.

    var resourceBuilder = ResourceBuilder.CreateDefault();
    configureResource(resourceBuilder);
    options.SetResourceBuilder(resourceBuilder);

    options.AddOtlpExporter(otlpOptions =>
    {
        // Use IConfiguration directly for Otlp exporter endpoint option.
        otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue(
              "Otlp:Endpoint", defaultValue: "http://localhost:4317")!);
    });
});


// Add services to the container.

appBuilder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
appBuilder.Services.AddEndpointsApiExplorer();
appBuilder.Services.AddSwaggerGen();
appBuilder.Services.AddSingleton<InfluxDbService>();

appBuilder.Services.Configure<InfluxDbSettings>(settings =>
{
    settings.Token = appBuilder.Configuration["Influxdb_Token"] ??  "nawl0Ot1SfHhY_0P8q4AIojN_UqE_cBHr432mG-C-7N6PmzbX_PQ5Vr5brMbe8leinQIGZ1osl8dH4V-rJThBg==";
    settings.IP = appBuilder.Configuration["Influxdb_IP"] ?? "localhost:8086";
});

var app = appBuilder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
