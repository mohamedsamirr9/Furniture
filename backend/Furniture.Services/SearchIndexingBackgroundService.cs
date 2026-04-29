using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Furniture.Servises_Abstraction;

namespace Furniture.Services;

public class SearchIndexingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SearchIndexingBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public SearchIndexingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<SearchIndexingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Search Indexing Background Service is starting.");

        // Wait a bit after startup to not interfere with initial boot
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Running background search indexing check...");
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
                    await recommendationService.IndexAllProductsAsync(onlyMissing: true);
                }

                _logger.LogInformation("Background search indexing check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during background search indexing.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
