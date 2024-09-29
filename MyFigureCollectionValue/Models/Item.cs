namespace MyFigureCollectionValue.Models
{
    public class Item
    {
        public Item()
        {
            this.AftermarketPrices = new HashSet<AftermarketPrice>();
            this.UserItems = new HashSet<UserItem>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Origin { get; set; }

        public string Company { get; set; }

        public decimal RetailPrice { get; set; }

        public string Currency { get; set; }

        public string Image { get; set; }

        public ICollection<AftermarketPrice> AftermarketPrices { get; set; }

        public ICollection<UserItem> UserItems { get; set; }
    }
}
