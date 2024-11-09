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

        public DbSet<CurrentAftermarketPrice> CurrentAftermarketPrices { get; set; }

        public DbSet<UserFigure> UserFigures { get; set; }

        public DbSet<UserFigureCollectionUrl> UserFigureCollectionUrls { get; set; }

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

            builder.Entity<Figure>()
                .HasMany(f => f.CurrentAftermarketPrices)
                .WithOne(cap => cap.Figure)
                .HasForeignKey(cap => cap.FigureId);

            builder.Entity<UserFigure>()
                .HasKey(uf => new { uf.UserId, uf.FigureId });

            builder.Entity<UserFigure>()
                .HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserId);

            builder.Entity<UserFigure>()
                .HasOne(uf => uf.Figure)
                .WithMany(f => f.UserFigures)
                .HasForeignKey(uf => uf.FigureId);

            builder.Entity<UserFigureCollectionUrl>()
                .HasIndex(u => u.UserId)
                .IsUnique();

            builder.Entity<UserFigureCollectionUrl>()
                .HasOne(u => u.User)
                .WithOne()
                .HasForeignKey<UserFigureCollectionUrl>(u => u.UserId);
        }
    }
}
