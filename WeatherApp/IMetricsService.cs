namespace WeatherApp;

public interface IMetricsService
{
    void AddForecast();
    public void AddExtremeForecast(string summary);



    public void UpdateAverageTemperature(WeatherForecast[] forecasts);
    
    

    public void RecordForecastProcessingTime(double processingTime);
}