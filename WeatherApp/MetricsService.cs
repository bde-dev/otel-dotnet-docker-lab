using System.Diagnostics.Metrics;

namespace WeatherApp;

public class MetricsService : IMetricsService
{
    // Counters
    private Counter<int> ForecastsCounter { get; }
    private Counter<int> ExtremeForecastsCounter { get; }
    
    
    // Gauges
    private ObservableGauge<int> SummariesGauge { get; }
    private static int SummariesCount = 5;
    private ObservableGauge<double> AverageTemperatureGauge { get; }
    private static double AverageTemperature { get; set; }
    
    
    // Histograms
    private Histogram<double> ForecastProcessingTime { get; }
    

    public MetricsService(IMeterFactory meterFactory)
    {
        // Create Meter
        var lMeter = meterFactory.Create("WeatherApp");
        
        // Create counters
        ForecastsCounter = lMeter.CreateCounter<int>("forecast_counter");
        ExtremeForecastsCounter = lMeter.CreateCounter<int>("extreme_forecast_counter");
        
        // Create gauges
        SummariesGauge = lMeter.CreateObservableGauge<int>("summaries_gauge", () => SummariesCount);
        AverageTemperatureGauge = lMeter.CreateObservableGauge("average_forecast_temperature_gauge", () => AverageTemperature);
        
        // Create histograms
        ForecastProcessingTime = lMeter.CreateHistogram<double>("forecast_processing_time");
    }
    
    // Counters interaction functions
    public void AddForecast() => ForecastsCounter.Add(1);
    
    public void AddExtremeForecast(string summary) => ExtremeForecastsCounter.Add(1,
        new KeyValuePair<string, object?>("forecast.summary", summary));
    
    
    // Gauges interaction functions
    public void UpdateAverageTemperature(WeatherForecast[] forecasts)
    {
        AverageTemperature = forecasts.Average(forecast => forecast.TemperatureC);
    }
    
    
    // Histogram interaction functions
    public void RecordForecastProcessingTime(double processingTime) => ForecastProcessingTime.Record(processingTime);
}