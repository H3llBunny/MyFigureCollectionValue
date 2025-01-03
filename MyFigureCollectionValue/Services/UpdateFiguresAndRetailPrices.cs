﻿
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

                            await DoWorkAsync(scraperService, figureService, figureUrls);

                            await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                        }

                        await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while executing the UpdateFiguresAndRetailPrices background task.");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task DoWorkAsync(
            IScraperService scraperService,
            IFigureService figureService,
            ICollection<string> figureUrls)
        {
            await scraperService.LoginAsync();

            var figures = new List<Figure>();
            var retailPrices = new List<RetailPrice>();

            var batches = figureUrls.Chunk(50);

            foreach (var batch in batches)
            {
                var (figureList, retailPriceList) = await scraperService.GetFiguresAndRetailPricesAsync(batch);

                if (figureList.Any())
                {
                    await figureService.UpdateFiguresAsync(figureList);
                }

                if (retailPriceList.Any())
                {
                    await figureService.UpdateRetailPricesAsync(retailPriceList);
                }

                await Task.Delay(TimeSpan.FromMinutes(2));
            }
        }
    }
}
