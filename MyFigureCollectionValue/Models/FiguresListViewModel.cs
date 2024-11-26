namespace MyFigureCollectionValue.Models
{
    public class FiguresListViewModel : PaginationViewModel
    {
        public string UserId { get; set; }

        public string UserFigureCollectionUrl { get; set; }

        public string FigureCollectionUsername { get; set; }

        public string SortOrder { get; set; }

        public decimal SumRetailPriceCollection { get; set; }

        public decimal SumAvgAftermarketPriceCollection { get; set; }

        public IEnumerable<FigureInListViewModel> Figures { get; set; }
    }
}
