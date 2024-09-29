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

        public DbSet<Item> Items { get; set; }

        public DbSet<AftermarketPrice> AftermarketPrices { get; set; }

        public DbSet<UserItem> UserItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Item>()
                .HasMany(i => i.AftermarketPrices)
                .WithOne(a => a.Item)
                .HasForeignKey(a => a.ItemId);

            builder.Entity<UserItem>()
                .HasKey(ui => new { ui.UserId, ui.ItemId });

            builder.Entity<UserItem>()
                .HasOne(ui => ui.User)
                .WithMany()
                .HasForeignKey(ui => ui.UserId);

            builder.Entity<UserItem>()
                .HasOne(ui => ui.Item)
                .WithMany(i => i.UserItems)
                .HasForeignKey(ui => ui.ItemId);
        }
    }
}
