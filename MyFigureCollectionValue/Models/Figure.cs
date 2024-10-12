using System.ComponentModel.DataAnnotations.Schema;

namespace MyFigureCollectionValue.Models
{
    public class Figure
    {
        public Figure()
        {
            this.AftermarketPrices = new HashSet<AftermarketPrice>();
            this.RetailPrices = new HashSet<RetailPrice>();
            this.UserItems = new HashSet<UserItem>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Origin { get; set; }

        public string Company { get; set; }

        public string Image { get; set; }

        public string FigureUrl { get; set; }

        public DateTime LastUpdated { get; set; }

        public ICollection<AftermarketPrice> AftermarketPrices { get; set; }

        public ICollection<RetailPrice> RetailPrices { get; set; }

        public ICollection<UserItem> UserItems { get; set; }
    }
}
