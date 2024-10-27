namespace MyFigureCollectionValue.Models
{
    public class FiguresListViewModel
    {
        public string OrderBy { get; set; }

        public decimal AvgRetailPriceOfCollection { get; set; }

        public decimal AvgAftermarketPriceOfCollection { get; set; }

        public IEnumerable<FigureInListViewModel> Figures { get; set; }
    }
}
