namespace MyFigureCollectionValue.Models
{
    public class AftermarketPrice
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public DateTime LoggedAt { get; set; }

        public int ItemId { get; set; }

        public Item Item { get; set; }
    }
}
