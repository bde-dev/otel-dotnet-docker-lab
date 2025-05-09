using MassTransit;

namespace WeatherApp;

public class WeatherPublisher(IBus bus, ILogger<WeatherPublisher> logger)
{
    public async Task PublishWeatherForecast(WeatherForecast forecast)
    {
        logger.LogInformation("Publishing weather forecast: {@forecast}", forecast);
        
        await bus.Publish(forecast);
        
        logger.LogInformation("Publishing weather forecast completed: {@forecast}", forecast);
    }
}