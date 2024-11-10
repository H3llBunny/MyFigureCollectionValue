using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Data
{
    public class ApplicationDbContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.LastUpdate.Any())
            {
                var initialData = new[]
                {
                    new LastUpdate { UpdateName = "FigureCollection", LastUpdated = new DateTime(1990, 1, 1) },
                    new LastUpdate { UpdateName = "RetailPrices", LastUpdated = new DateTime(1990, 1, 1) },
                    new LastUpdate { UpdateName = "AftermarketPrices", LastUpdated = new DateTime(1990, 1, 1) },
                };

                await context.LastUpdate.AddRangeAsync(initialData);
                await context.SaveChangesAsync();
            }
        }
    }
}
