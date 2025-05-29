

using CoreBookingPlatform.AdapterService.Interfaces;
using Polly;

namespace CoreBookingPlatform.AdapterService.Services
{
    public class StartupImportService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StartupImportService> _logger;

        public StartupImportService(IServiceProvider serviceProvider, ILogger<StartupImportService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting automatic product and content import...");

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(5),
                    (ex, time, attempt, context) =>
                    {
                        _logger.LogWarning("Retry attempt {Attempt} after error: {Error}", attempt, ex.Message);
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var adapters = scope.ServiceProvider.GetServices<IAdapter>();
                foreach (var adapter in adapters)
                {
                    _logger.LogInformation("Importing data for {ExternalSystemName}", adapter.ExternalSystemName);
                    await adapter.ImportProductsAsync();
                }
            });

            _logger.LogInformation("Automatic import completed.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AdapterService shutting down gracefully.");
            return Task.CompletedTask;
        }
    }
}
