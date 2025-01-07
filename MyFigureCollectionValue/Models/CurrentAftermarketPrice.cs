using System.ComponentModel.DataAnnotations.Schema;

namespace MyFigureCollectionValue.Models
{
    public class CurrentAftermarketPrice
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public DateTime LoggedAt { get; set; }

        public int FigureId { get; set; }

        public Figure Figure { get; set; }

        public string? Url { get; set; }
    }
}
