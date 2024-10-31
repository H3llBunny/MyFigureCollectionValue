using Microsoft.Extensions.Options;
using MyFigureCollectionValue.Models;
using System.Text.Json;

namespace MyFigureCollectionValue.Services
{
    public class AutoUpdateDatabaseService : BackgroundService
    {
        private readonly CurrencyFreaksSettings _fixerSettings;
        private readonly ILogger<AutoUpdateDatabaseService> _logger;
        private readonly string currencies = "eur,gbp,jpy,aud,cad,hkd";

        public AutoUpdateDatabaseService(IOptions<CurrencyFreaksSettings> fixerSettings, ILogger<AutoUpdateDatabaseService> logger)
        {
            this._fixerSettings = fixerSettings.Value;
            this._logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync();

                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "An error occured while executing the background task.");

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task DoWorkAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string baseUrl = this._fixerSettings.BaseUrl;
                string apiKey = this._fixerSettings.ApiKey;

                string url = $"{baseUrl}?apikey={apiKey}&symbols={currencies}";

                HttpResponseMessage response = await client.GetAsync(url);

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
                    this._logger.LogWarning("Failed to retrieve exchange rates or received an unsuccessful response.");
                }
            }
        }
    }
}
