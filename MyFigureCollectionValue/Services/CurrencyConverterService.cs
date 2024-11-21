using MyFigureCollectionValue.Models;
using System.Globalization;
using System.Text.Json;

namespace MyFigureCollectionValue.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly string filePath = Path.Combine(AppContext.BaseDirectory, "exchange_rates.json");

        public async Task<ICollection<RetailPrice>> ConvertRetailPricesToUSDAsync(ICollection<RetailPrice> retailPrices)
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
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

        public async Task<ICollection<T>> ConvertAftermarketPricesToUSDAsync<T>(ICollection<T> aftermarketPrices) where T : class
        {
            var jsonContent = File.ReadAllText(filePath);
            var exchangeRatesUSD = JsonSerializer.Deserialize<ExchangeRate>(jsonContent);

            var priceProperty = typeof(T).GetProperty("Price");
            var currencyProperty = typeof(T).GetProperty("Currency");

            if (priceProperty == null || currencyProperty == null)
            {
                throw new ArgumentException($"The type {typeof(T).Name} does not have the required properties 'Price' and 'Currency'.");
            }

            foreach (var price in aftermarketPrices)
            {
                var currency = currencyProperty.GetValue(price)?.ToString();
                if (currency == "$")
                {
                    currencyProperty.SetValue(price, "USD");
                    continue;
                }

                string currencyCode = MapSymbolToCode(currency);

                if (exchangeRatesUSD.Rates.TryGetValue(currencyCode, out var exchangeRate) &&
                                   decimal.TryParse(exchangeRate, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
                {
                    var currentPrice = (decimal)priceProperty.GetValue(price);
                    priceProperty.SetValue(price, Math.Round(currentPrice / rate, 2));
                    currencyProperty.SetValue(price, "USD");
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
