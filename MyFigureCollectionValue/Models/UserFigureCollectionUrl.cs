using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyFigureCollectionValue.Models
{
    public class UserFigureCollectionUrl
    {
        [Key]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        public string FigureCollectionUrl { get; set; }

        public DateTime LastRefreshed { get; set; }
    }
}
