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

        public DbSet<AftermarketPrice> AftermarketPrices { get; set; }

        public DbSet<UserItem> UserItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Figure>()
                .HasMany(i => i.AftermarketPrices)
                .WithOne(a => a.Figure)
                .HasForeignKey(a => a.FigureId);

            builder.Entity<UserItem>()
                .HasKey(ui => new { ui.UserId, ui.FigureId });

            builder.Entity<UserItem>()
                .HasOne(ui => ui.User)
                .WithMany()
                .HasForeignKey(ui => ui.UserId);

            builder.Entity<UserItem>()
                .HasOne(ui => ui.Figure)
                .WithMany(i => i.UserItems)
                .HasForeignKey(ui => ui.FigureId);
        }
    }
}
