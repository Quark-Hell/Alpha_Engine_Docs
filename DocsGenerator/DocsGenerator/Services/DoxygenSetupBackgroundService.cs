namespace DocsGenerator.Services;

public class DoxygenSetupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DoxygenSetupBackgroundService> _logger;

    public DoxygenSetupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DoxygenSetupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting Doxygen setup...");

            using var scope = _serviceProvider.CreateScope();
            var setupService = scope.ServiceProvider.GetRequiredService<DoxygenSetupService>();

            await setupService.SetupAsync();

            _logger.LogInformation("Doxygen setup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during Doxygen setup");
            throw;
        }
    }
}