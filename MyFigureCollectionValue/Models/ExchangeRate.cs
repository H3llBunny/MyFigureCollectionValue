using System.Text.Json.Serialization;

namespace MyFigureCollectionValue.Models
{
    public class ExchangeRate
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("base")]
        public string Base { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, string> Rates { get; set; }
    }
}
