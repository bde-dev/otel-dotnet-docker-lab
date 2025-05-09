using System.Diagnostics;
using MassTransit;

namespace WeatherApp;

public class WeatherService
{
    private readonly ILogger<WeatherService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IMetricsService _metricsService;
    private readonly IBus _bus;

    public WeatherService(ILogger<WeatherService> logger, HttpClient httpClient, IMetricsService metricsService, IBus bus)
    {
        _logger = logger;
        _httpClient = httpClient;
        _metricsService = metricsService;
        _bus = bus;
    }

    public async Task<WeatherForecast[]> GetWeatherForecast()
    {
        var lActivitySource = new ActivitySource("MyActivitySource");
        var lActivity = lActivitySource.StartActivity("MyActivity");
        
        //#############################################
        // Simulate an external HTTP API request
        // Merely exists to add a span to the trace
        // The response data is discarded
        //#############################################
        
        _logger.LogInformation("Calling openweathermap API");
        
        var apiKey = "04c17c216536cfd47706a94c615d8c58";

        var url = $"https://api.openweathermap.org/data/2.5/weather?q=london&appid={apiKey}&units=metric";
        
        _logger.LogInformation("Request URL: {@url}", url);

        var response = await _httpClient.GetAsync(url);
        
        var content = await response.Content.ReadAsStringAsync();
        
        //#############################################
        // End arbitrary external HTTP API request
        //#############################################
        
        
        // Generate forecasts array
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var lForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();
        
        _logger.LogInformation("Got forecasts: {@lForecasts}", lForecasts);

        
        // Publish to RabbitMQ
        await _bus.Publish(lForecasts);
        
        
        lActivity?.Stop();
        
        // Forecasts successfully generated - generate metrics
        _metricsService.RecordForecastProcessingTime(lActivity.Duration.TotalMilliseconds);
        
        _metricsService.AddForecast();

        _metricsService.UpdateAverageTemperature(lForecasts);

        var lExtremeForecasts = lForecasts.Where(forecast => forecast.Summary is "Freezing" or "Sweltering");
        if (lExtremeForecasts.Any())
        {
            foreach (var lExtremeForecast in lExtremeForecasts)
            {
                _metricsService.AddExtremeForecast(lExtremeForecast.Summary);
            }
        }
            
        return lForecasts;
    }
}