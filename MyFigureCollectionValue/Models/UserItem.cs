using Microsoft.AspNetCore.Identity;

namespace MyFigureCollectionValue.Models
{
    public class UserItem
    {
        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        public int FigureId { get; set; }

        public Figure Figure { get; set; }
    }
}
