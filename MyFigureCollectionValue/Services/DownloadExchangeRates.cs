using Microsoft.Extensions.Options;
using MyFigureCollectionValue.Models;
using System.Text.Json;

namespace MyFigureCollectionValue.Services
{
    public class DownloadExchangeRates : BackgroundService
    {
        private readonly CurrencyFreaksSettings _fixerSettings;
        private readonly ILogger<DownloadExchangeRates> _logger;
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string currencies = "eur,gbp,jpy,aud,cad,hkd,cny,idr,krw,sgd,twd,aed";

        public DownloadExchangeRates(
            IOptions<CurrencyFreaksSettings> fixerSettings,
            ILogger<DownloadExchangeRates> logger,
            HttpClient httpClient,
            IServiceScopeFactory scopeFactory)
        {
            this._fixerSettings = fixerSettings.Value;
            this._logger = logger;
            this._httpClient = httpClient;
            this._scopeFactory = scopeFactory;
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
                        var lastUpdated = await lastUpdateService.GetLastDateForExchangeRateUpdateAsync();

                        if ((DateTime.UtcNow - lastUpdated).TotalDays >= 1)
                        {
                            await DoWorkAsync(scope);
                        }
                    }

                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "An error occured while executing the background task.");

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task DoWorkAsync(IServiceScope scope)
        {
            var lastUpdateService = scope.ServiceProvider.GetRequiredService<ILastUpdateService>();

            string baseUrl = this._fixerSettings.BaseUrl;
            string apiKey = this._fixerSettings.ApiKey;

            string url = $"{baseUrl}?apikey={apiKey}&symbols={currencies}";

            HttpResponseMessage response = await this._httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync();
            var jsonContent = JsonSerializer.Deserialize<ExchangeRate>(jsonString);

            if (jsonContent != null)
            {
                string jsonOutput = JsonSerializer.Serialize(jsonContent, new JsonSerializerOptions { WriteIndented = true });

                string filePath = Path.Combine(AppContext.BaseDirectory, "exchange_rates.json");
                await File.WriteAllTextAsync(filePath, jsonOutput);

                await lastUpdateService.UpdateLastExchangeRateDateAsync();
            }
            else
            {
                this._logger.LogWarning("Failed to retrieve exchange rates or received an unsuccessful response.");
            }
        }
    }
}
