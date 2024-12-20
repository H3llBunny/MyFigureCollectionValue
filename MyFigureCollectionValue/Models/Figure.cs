﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MyFigureCollectionValue.Models
{
    public class Figure
    {
        public Figure()
        {
            this.AftermarketPrices = new HashSet<AftermarketPrice>();
            this.CurrentAftermarketPrices = new HashSet<CurrentAftermarketPrice>();
            this.RetailPrices = new HashSet<RetailPrice>();
            this.UserFigures = new HashSet<UserFigure>();
            this.UserPurchasePrices = new HashSet<UserPurchasePrices>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Origin { get; set; }

        public string Company { get; set; }

        public string Image { get; set; }

        public string FigureUrl { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime LastUpdatedAftermarketPrices { get; set; }

        public ICollection<AftermarketPrice> AftermarketPrices { get; set; }

        public ICollection<CurrentAftermarketPrice> CurrentAftermarketPrices { get; set; }

        public ICollection<RetailPrice> RetailPrices { get; set; }

        public ICollection<UserFigure> UserFigures { get; set; }

        public ICollection<UserPurchasePrices> UserPurchasePrices { get; set; }
    }
}
