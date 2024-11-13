using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyFigureCollectionValue.Models
{
    public class AftermarketPrice
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public DateTime LoggedAt { get; set; }

        public DateTime LastUpdated { get; set; }

        public int FigureId { get; set; }

        public Figure Figure { get; set; }
    }
}
