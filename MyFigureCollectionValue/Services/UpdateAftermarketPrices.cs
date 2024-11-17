using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public class UpdateAftermarketPrices : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UpdateAftermarketPrices> _logger;

        public UpdateAftermarketPrices(
            IServiceScopeFactory scopeFactory,
            ILogger<UpdateAftermarketPrices> logger)
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

                        var figureUrlAndIds = await figureService.GetFigureUrlsWithOutdatedAftermarketPricesAsync();

                        if (figureUrlAndIds.Any())
                        {
                            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
                            var currencyConverterService = scope.ServiceProvider.GetRequiredService<ICurrencyConverterService>();

                            await DoWorkAsyn(scraperService, figureService, currencyConverterService, figureUrlAndIds);

                            await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
                        }

                        await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while executing the UpdateAftermarketPrices background task.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task DoWorkAsyn(
            IScraperService scraperService, 
            IFigureService figureService,
            ICurrencyConverterService currencyConverterService,
            Dictionary<string, int> figureUrlAndIds)
        {
            await scraperService.LoginAsync();

            var aftermarketPrices = new List<AftermarketPrice>();

            foreach (var figure in figureUrlAndIds)
            {
                var newAftermarketPrices = await scraperService.GetAftermarketPriceListAsync(figure.Key, figure.Value);

                if (newAftermarketPrices != null)
                {
                    aftermarketPrices.AddRange(newAftermarketPrices);
                }
            }

            if (aftermarketPrices.Any())
            {
                var aftermarketPricesInUSD = currencyConverterService.ConvertAftermarketPricesToUSD(aftermarketPrices);

                await figureService.AddAftermarketPricesAsync(aftermarketPricesInUSD);

                var currentAftermarketPrices = aftermarketPricesInUSD.Select(ap => new CurrentAftermarketPrice
                {
                    Id = ap.Id,
                    Price = ap.Price,
                    Currency = ap.Currency,
                    LoggedAt = ap.LoggedAt,
                    FigureId = ap.FigureId,
                });

                await figureService.AddCurrentAftermarketPricesAsync(currentAftermarketPrices);

                var figureIds = figureUrlAndIds.Select(f => f.Value).ToList();

                await figureService.UpdateFiguresLastUpdatedRetailPricesAsync(figureIds);
            }
        }
    }
}
