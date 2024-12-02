using Microsoft.AspNetCore.Identity;

namespace MyFigureCollectionValue.Models
{
    public class UserPurchasePrices
    {
        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        public int FigureId { get; set; }

        public Figure Figure { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }
    }
}
