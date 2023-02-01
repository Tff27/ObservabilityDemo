using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ObservabilityDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        // Use the ILogger interface that derives from your specific type 
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger; // Make sure your logger is initialized by DI
        }

        [HttpGet("GetWeatherForecastWithLogs", Name = "GetWeatherForecastWithLogs")]
        public IEnumerable<WeatherForecast> GetWithLogs()
        {
            // Add controller information to log scope
            using (_logger.BeginScope("WeatherForecastController Scope"))
            {
                // Log with different levels
                _logger.LogInformation("Get a weather forecast with logs");
                _logger.LogWarning("Get a weather forecast with logs");
                _logger.LogError("Get a weather forecast with logs");
                // There are other levels available such as Debug, Critical and Trace

                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
            }
        }

        // Create Meter
        private static Meter meter = new Meter(Constants.MeterName, "v1.0");
        // Create counter
        private static Counter<int> counter = meter.CreateCounter<int>("Requests");

        [HttpGet("GetWeatherForecastWithMetrics", Name = "GetWeatherForecastWithMetrics")]
        public IEnumerable<WeatherForecast> GetWithMetrics()
        {
            // Increase the metric counter
            counter.Add(1);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        // Create a new activity source
        private static ActivitySource source = new ActivitySource(Constants.ActivitySourceName, "1.0.0");

        [HttpGet("GetWeatherForecastWithTracing", Name = "GetWeatherForecastWithTracing")]
        public IEnumerable<WeatherForecast> GetWithTracing()
        {
            // Create a new activity
            using Activity activity = source.StartActivity("GetWeatherForecastWithTracing", ActivityKind.Internal);
            activity?.AddTag("Method", "GetWeatherForecastWithTracing");

            var wf1 = GenerateWeatherForecast("Get WeatherForecast (1)");
            var wf2 = GenerateWeatherForecast("Get WeatherForecast (2)");

            // Signal that activity ended
            activity?.Stop();

            return wf1.Concat(wf2).ToArray();
        }

        static WeatherForecast[] GenerateWeatherForecast(string activityName)
        {
            // Create a new activity
            using Activity activity = source.StartActivity(activityName, ActivityKind.Internal);

            var weatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
             .ToArray();

            // Signal that activity ended
            activity?.Stop();

            return weatherForecasts;
        }
    }
}