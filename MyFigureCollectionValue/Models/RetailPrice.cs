using System.ComponentModel.DataAnnotations;

namespace MyFigureCollectionValue.Models
{
    public class RetailPrice
    {
        [Key]
        public int Id { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public DateTime ReleaseDate { get; set; }

        public DateTime LastUpdated { get; set; }

        public int FigureId { get; set; }

        public Figure Figure { get; set; }
    }
}
