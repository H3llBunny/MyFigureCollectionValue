namespace MyFigureCollectionValue.Models
{
    public class FiguresListViewModel
    {
        public int FiguresPerPage { get; set; }

        public int PageNumber { get; set; }

        public string UserId { get; set; }

        public int FiguresCount { get; set; }

        public string OrderBy { get; set; }

        public decimal AvgRetailPriceOfCollection { get; set; }

        public decimal AvgAftermarketPriceOfCollection { get; set; }

        public IEnumerable<FigureInListViewModel> Figures { get; set; }
    }
}
