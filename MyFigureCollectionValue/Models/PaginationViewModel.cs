namespace MyFigureCollectionValue.Models
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }

        public bool HasPreviousPage => PageNumber > 1;

        public int PreviousPageNumber => PageNumber - 1;

        public bool HasNextPage => PageNumber < PagesCount;

        public int NextPageNumber => PageNumber + 1;

        public int PagesCount => (int)Math.Ceiling((double)FiguresCount / FiguresPerPage);

        public int FiguresCount { get; set; }

        public int FiguresPerPage { get; set; }
    }
}
