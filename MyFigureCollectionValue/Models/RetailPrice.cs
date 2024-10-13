using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyFigureCollectionValue.Models
{
    public class RetailPrice
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int FigureId { get; set; }

        public Figure Figure { get; set; }
    }
}
