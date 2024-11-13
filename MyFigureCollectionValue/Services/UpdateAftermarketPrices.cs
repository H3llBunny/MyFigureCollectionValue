
namespace MyFigureCollectionValue.Services
{
    public class UpdateAftermarketPrices : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UpdateAftermarketPrices> _logger;

        public UpdateAftermarketPrices(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<UpdateAftermarketPrices> logger)
        {
            this._scopeFactory = serviceScopeFactory;
            this._logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = this._scopeFactory.CreateScope())
                    {
                        var lastUpdateService = scope.ServiceProvider.GetRequiredService<ILastUpdateService>();
                        var lastUpdated = await lastUpdateService.GetLastDateForAfterrmarketPricesUpdateAsync();

                        if ((DateTime.UtcNow - lastUpdated).TotalDays >= 1)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "An error occured while executing the UpdateAftermarketPrices background task.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
