using MyFigureCollectionValue.Models;
using System.Text.Json;

namespace MyFigureCollectionValue.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        public ICollection<AftermarketPrice> ConvertAftermarketPricesToUSD(ICollection<AftermarketPrice> aftermarketPrices)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "exchange_rates.json");
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
                                      decimal.TryParse(exchangeRate, out var rate))
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
