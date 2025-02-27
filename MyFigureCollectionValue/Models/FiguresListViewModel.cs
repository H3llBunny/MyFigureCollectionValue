﻿namespace MyFigureCollectionValue.Models
{
    public class FiguresListViewModel : PaginationViewModel
    {
        public string UserId { get; set; }

        public string UserFigureCollectionUrl { get; set; }

        public string FigureCollectionUsername { get; set; }

        public string SortOrder { get; set; }

        public decimal SumRetailPriceCollection { get; set; }

        public decimal SumAvgAftermarketPriceCollection { get; set; }

        public decimal TotalPaid { get; set; }

        public bool ShouldCalcPaidPricePercent { get; set; }

        public DateTime LastRefreshCollection { get; set; }

        public IEnumerable<FigureInListViewModel> Figures { get; set; }
    }
}
