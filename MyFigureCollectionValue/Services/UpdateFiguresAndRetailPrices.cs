
namespace MyFigureCollectionValue.Services
{
    public class UpdateFiguresAndRetailPrices : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UpdateFiguresAndRetailPrices> _logger;

        public UpdateFiguresAndRetailPrices(
            IServiceScopeFactory scopeFactory,
            ILogger<UpdateFiguresAndRetailPrices> logger)
        {
            this._scopeFactory = scopeFactory;
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
                        var figureService = scope.ServiceProvider.GetRequiredService<IFigureService>();
                        var figureUrls = await figureService.GetOutdatedFigureUrlsAsync();

                        if (figureUrls.Any())
                        {
                            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
                            var currencyConverterService = scope.ServiceProvider.GetRequiredService<ICurrencyConverterService>();

                            await DoWorkAsync(scraperService, currencyConverterService, figureUrls);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "An error occured while executing the UpdateFiguresAndRetailPrices background task.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task DoWorkAsync(IScraperService scraperService, ICurrencyConverterService currencyConverterService, List<string> figureUrls)
        {
        }
    }
}
