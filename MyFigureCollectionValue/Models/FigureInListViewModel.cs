namespace MyFigureCollectionValue.Models

{
    public class FigureInListViewModel
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; }

        public string Name { get; set; }

        public decimal RetailPrice { get; set; }

        public string RetailPriceCurrency { get; set; }

        public AftermarketPrice LowestAftermarketPeice { get; set; }

        public AftermarketPrice HighestAftermarketPrice { get; set; }

        public decimal AvgCurrentAftermarketPrice { get; set; }

        public string AvgAftermarketPriceCurrency { get; set; }

        public ICollection<AftermarketPrice> AftermarketPrices { get; set; }
    }
}
