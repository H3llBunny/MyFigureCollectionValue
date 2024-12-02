namespace MyFigureCollectionValue.Models
{
    public class FigurePageViewModel : FigureInListViewModel
    {
        public string Origin { get; set; }

        public string Company { get; set; }

        public string FigureUrl { get; set; }

        public ICollection<string> SupportedCurrencies { get; set; }
    }
}
