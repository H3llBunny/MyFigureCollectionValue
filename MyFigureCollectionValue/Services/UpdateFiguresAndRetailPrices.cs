
using MyFigureCollectionValue.Models;

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
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var figureService = scope.ServiceProvider.GetRequiredService<IFigureService>();
                        var figureUrls = await figureService.GetOutdatedFigureUrlsAsync();

                        if (figureUrls.Any())
                        {
                            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
                            var currencyConverterService = scope.ServiceProvider.GetRequiredService<ICurrencyConverterService>();

                            await DoWorkAsync(scraperService, currencyConverterService, figureService, figureUrls);

                            await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                        }

                        await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while executing the UpdateFiguresAndRetailPrices background task.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task DoWorkAsync(
            IScraperService scraperService,
            ICurrencyConverterService currencyConverterService,
            IFigureService figureService,
            ICollection<string> figureUrls)
        {
            await scraperService.LoginAsync();

            var figures = new List<Figure>();
            var retailPrices = new List<RetailPrice>();

            var (figureList, retailPriceList) = await scraperService.GetFiguresAndRetailPricesAsync(figureUrls);

            if (figureList.Any())
            {
                await figureService.UpdateFiguresAsync(figureList);
            }

            if (retailPriceList.Any())
            {
                await figureService.UpdateRetailPricesAsync(retailPriceList);
            }
        }
    }
}
