using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Figure> Figures { get; set; }

        public DbSet<RetailPrice> RetailPrices { get; set; }

        public DbSet<AftermarketPrice> AftermarketPrices { get; set; }

        public DbSet<UserFigure> UserFigure { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Figure>()
                .HasMany(f => f.RetailPrices)
                .WithOne(rp => rp.Figure)
                .HasForeignKey(rp => rp.FigureId);

            builder.Entity<Figure>()
                .HasMany(f => f.AftermarketPrices)
                .WithOne(ap => ap.Figure)
                .HasForeignKey(ap => ap.FigureId);

            builder.Entity<UserFigure>()
                .HasKey(uf => new { uf.UserId, uf.FigureId });

            builder.Entity<UserFigure>()
                .HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserId);

            builder.Entity<UserFigure>()
                .HasOne(uf => uf.Figure)
                .WithMany(f => f.UserItems)
                .HasForeignKey(uf => uf.FigureId);
        }
    }
}
