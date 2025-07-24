using System.Reflection;
using MassTransit;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using WeatherApp;
using WeatherApp.Components;

var builder = WebApplication.CreateBuilder(args);

var rabbitHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "localhost";
var otelCollectorHostname = Environment.GetEnvironmentVariable("OTEL_COLLECTOR_HOSTNAME") ?? "localhost";
var otelGrpcEndpoint = "http://" + otelCollectorHostname + ":4317";
var otelHttpEndpoint = "http://" + otelCollectorHostname + ":4318";

Console.WriteLine("Got env variables: ");
Console.WriteLine($"rabbitHostname: {rabbitHostname}");
Console.WriteLine($"otelCollectorHostname: {otelCollectorHostname}");
Console.WriteLine($"otelGrpcEndpoint: {otelGrpcEndpoint}");
Console.WriteLine($"otelHttpEndpoint: {otelHttpEndpoint}");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddTransient<WeatherPublisher>();
builder.Services.AddSingleton<IMetricsService, MetricsService>();



builder.Services.AddMassTransit(mt =>
{
    mt.SetKebabCaseEndpointNameFormatter();
    mt.AddConsumers(typeof(Program).Assembly);
    mt.UsingRabbitMq((context, config) =>
    {
        config.Host(rabbitHostname, host =>
        {
            host.Username("guest");
            host.Password("guest");
        });
        
        config.ConfigureEndpoints(context);
    });
});



//--------------------------\\
// Begin OTLP configuration \\
//--------------------------\\

builder.Host.UseSerilog((context, logging) =>
{
    //---------------------------------------\\
    // General Serilog logging configuration \\
    //---------------------------------------\\
    logging
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString())
        
        .WriteTo.Console()
        
        //------------------------------------\\
        // Serilog OTLP logging configuration \\
        //------------------------------------\\
        .WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = otelHttpEndpoint;
            options.Protocol = OtlpProtocol.HttpProtobuf;
            options.ResourceAttributes = new Dictionary<string, object>
            {
                { "service.namespace", "weather" },
                { "service.name", "WeatherApp" }
            };
        });
});


//-----------------------------\\
// OTLP .NET SDK configuration \\
//-----------------------------\\

// The below configuration uses the .NET open telemetry SDK
// and the OpenTelemetry.Extension.Hosting package.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        resource.AddAttributes(new Dictionary<string, object>
        {
            { "service.namespace", "weather" },
            { "service.name", "WeatherApp" }
        });
        resource.AddService("WeatherApp");
        resource.AddTelemetrySdk();
    })
    
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("MassTransit")
            .AddSource("Npgsql")
            .AddSource("MyActivitySource")
            
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();

        tracing.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otelGrpcEndpoint);
            options.Protocol = OtlpExportProtocol.Grpc;
        });
    })
    
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()

            // Built-in metrics
            //.AddMeter("Microsoft.AspNetCore.Hosting")
            //.AddMeter("Microsoft.AspNet.Server.Kestrel")
            
            
            // App metrics
            .AddMeter("WeatherApp")
            

            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otelGrpcEndpoint);
                opts.Protocol = OtlpExportProtocol.Grpc;
            });
    });

//------------------------\\
// End OTLP configuration \\
//------------------------\\





var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();