using System.ComponentModel.DataAnnotations;

namespace MyFigureCollectionValue.Models
{
    public class LastUpdate
    {
        [Key]
        public int Id { get; set; }

        public string UpdateName { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
