using MyFigureCollectionValue.Models;
using System.Globalization;
using System.Text.Json;

namespace MyFigureCollectionValue.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly string filePath = Path.Combine(AppContext.BaseDirectory, "exchange_rates.json");

        public ICollection<RetailPrice> ConvertRetailPricesToUSD(ICollection<RetailPrice> retailPrices)
        {
            var jsonContent = File.ReadAllText(filePath);
            var exchangeRatesUSD = JsonSerializer.Deserialize<ExchangeRate>(jsonContent);

            foreach (var price in retailPrices)
            {
                if (price.Currency == "USD")
                {
                    continue;
                }

                string currencyCode = price.Currency;

                if (exchangeRatesUSD.Rates.TryGetValue(currencyCode, out var exchangeRateString) &&
                                   decimal.TryParse(exchangeRateString, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
                {
                    price.Price = Math.Round(price.Price / rate, 2);
                    price.Currency = "USD";
                }
                else
                {
                    throw new KeyNotFoundException($"Exchange rate for {currencyCode} not found in the data.");
                }
            }

            return retailPrices;
        }

        public ICollection<AftermarketPrice> ConvertAftermarketPricesToUSD(ICollection<AftermarketPrice> aftermarketPrices)
        {
            var jsonContent = File.ReadAllText(filePath);
            var exchangeRatesUSD = JsonSerializer.Deserialize<ExchangeRate>(jsonContent);

            foreach (var price in aftermarketPrices)
            {
                if (price.Currency == "$")
                {
                    price.Currency = "USD";
                    continue;
                }

                string currencyCode = MapSymbolToCode(price.Currency);

                if (exchangeRatesUSD.Rates.TryGetValue(currencyCode, out var exchangeRate) &&
                                   decimal.TryParse(exchangeRate, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
                {
                    price.Price = Math.Round(price.Price / rate, 2);
                    price.Currency = "USD";
                }
                else
                {
                    throw new KeyNotFoundException($"Exchange rate for {currencyCode} not found in the data.");
                }
            }

            return aftermarketPrices;
        }

        private string MapSymbolToCode(string currencySymbol)
        {
            return currencySymbol switch
            {
                "€" => "EUR",
                "A$" => "AUD",
                "C$" => "CAD",
                "£" => "GBP",
                "HK$" => "HKD",
                "¥" => "JPY",
                "$" => "USD",
                _ => throw new ArgumentException("Unsupported currency symbol")
            };
        }
    }
}
