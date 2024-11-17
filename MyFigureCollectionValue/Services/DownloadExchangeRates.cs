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
        private readonly string currencies = "eur,gbp,jpy,aud,cad,hkd,cny,idr,krw,sgd,twd,aed";

        public DownloadExchangeRates(
            IOptions<CurrencyFreaksSettings> fixerSettings,
            ILogger<DownloadExchangeRates> logger,
            HttpClient httpClient)
        {
            _fixerSettings = fixerSettings.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string filePath = Path.Combine(AppContext.BaseDirectory, "exchange_rates.json");

                    if (File.Exists(filePath))
                    {
                        string jsonString = await File.ReadAllTextAsync(filePath);
                        var existingDate = JsonSerializer.Deserialize<ExchangeRate>(jsonString);

                        DateTime.TryParse(existingDate.Date, out DateTime lastUdated);

                        if ((DateTime.UtcNow - lastUdated).TotalDays >= 1)
                        {
                            await DoWorkAsync();
                            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                        }
                    }
                    else
                    {
                        await DoWorkAsync();
                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while executing the DownloadExchangeRates background task.");

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task DoWorkAsync()
        {
            string baseUrl = _fixerSettings.BaseUrl;
            string apiKey = _fixerSettings.ApiKey;

            string url = $"{baseUrl}?apikey={apiKey}&symbols={this.currencies}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync();
            var jsonContent = JsonSerializer.Deserialize<ExchangeRate>(jsonString);

            if (jsonContent != null)
            {
                string jsonOutput = JsonSerializer.Serialize(jsonContent, new JsonSerializerOptions { WriteIndented = true });

                string filePath = Path.Combine(AppContext.BaseDirectory, "exchange_rates.json");
                await File.WriteAllTextAsync(filePath, jsonOutput);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve exchange rates or received an unsuccessful response.");
            }
        }
    }
}
