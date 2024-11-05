namespace MyFigureCollectionValue.Models
{
    public class FiguresListViewModel
    {
        public int FiguresPerPage { get; set; }

        public int PageNumber { get; set; }

        public string UserId { get; set; }

        public int FiguresCount { get; set; }

        public string OrderBy { get; set; }

        public string UserFigureCollectionUrl { get; set; }

        public string FigureCollectionUsername { get; set; }

        public decimal SumRetailPriceCollection { get; set; }

        public decimal SumAvgAftermarketPriceCollection { get; set; }

        public IEnumerable<FigureInListViewModel> Figures { get; set; }
    }
}
