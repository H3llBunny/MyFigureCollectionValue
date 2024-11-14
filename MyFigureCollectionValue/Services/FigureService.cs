using Microsoft.EntityFrameworkCore;
using MyFigureCollectionValue.Data;
using MyFigureCollectionValue.Models;

namespace MyFigureCollectionValue.Services
{
    public class FigureService : IFigureService
    {
        private readonly ApplicationDbContext _dbContext;

        public FigureService(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<bool> DoesFigureExistAsync(int id)
        {
            return await this._dbContext.Figures.AnyAsync(f => f.Id == id);
        }

        public async Task AddFiguresAsync(IEnumerable<Figure> figures)
        {
            await this._dbContext.AddRangeAsync(figures);
            await this._dbContext.SaveChangesAsync();
        }

        public async Task AddRetailPricesAsync(IEnumerable<RetailPrice> retailPrices)
        {
            await this._dbContext.AddRangeAsync(retailPrices);
            await this._dbContext.SaveChangesAsync();
        }

        public async Task AddCurrentAftermarketPricesAsync(IEnumerable<CurrentAftermarketPrice> currentAftermarketPrices)
        {
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM CurrentAftermarketPrices");

            await this._dbContext.AddRangeAsync(currentAftermarketPrices);
            await this._dbContext.SaveChangesAsync();
        }

        public async Task AddAftermarketPricesAsync(IEnumerable<AftermarketPrice> aftermarketPrices)
        {
            var priceIds = aftermarketPrices.Select(ap => ap.Id).ToList();

            var existingPriceIds = await this._dbContext.AftermarketPrices
                .Where(ap => priceIds.Contains(ap.Id))
                .Select(ap => ap.Id)
                .ToListAsync();

            var newPrices = aftermarketPrices.Where(ap => !existingPriceIds.Contains(ap.Id)).ToList();

            if (newPrices.Any())
            {
                await this._dbContext.AftermarketPrices.AddRangeAsync(newPrices);
                await this._dbContext.SaveChangesAsync();
            }

            var existingAds = await this._dbContext.AftermarketPrices.Where(ap => existingPriceIds.Contains(ap.Id)).ToListAsync();

            bool priceUpdated = false;

            foreach (var existingAd in existingAds)
            {
                var newPriceAd = aftermarketPrices.FirstOrDefault(ap => ap.Id == existingAd.Id);

                if (newPriceAd != null && existingAd.Price != newPriceAd.Price)
                {
                    existingAd.Price = newPriceAd.Price;
                    priceUpdated = true;
                }
            }

            if (priceUpdated)
            {
                await this._dbContext.SaveChangesAsync();
            }
        }

        public async Task AddUserFiguresAsync(string userId, IEnumerable<Figure> figureList)
        {
            var userFigures = figureList.Select(figure => new UserFigure
            {
                UserId = userId,
                FigureId = figure.Id,
            }).ToList();

            await this._dbContext.UserFigures.AddRangeAsync(userFigures);
            await this._dbContext.SaveChangesAsync();
        }

        public async Task RemoveUserFiguresAsync(string userId)
        {
            var userFigures = this._dbContext.UserFigures.Where(uf => uf.UserId == userId);

            this._dbContext.UserFigures.RemoveRange(userFigures);

            await this._dbContext.SaveChangesAsync();
        }

        public async Task AddExistingFigureToUserAsync(int figureId, string userId)
        {
            var userFigure = new UserFigure
            {
                UserId = userId,
                FigureId = figureId
            };

            await this._dbContext.UserFigures.AddAsync(userFigure);
            await this._dbContext.SaveChangesAsync();
        }

        public async Task<int> GetUserFiguresCountAsync(string userId)
        {
            return await this._dbContext.UserFigures.Where(u => u.UserId == userId).CountAsync();
        }

        public async Task<IEnumerable<FigureInListViewModel>> GetAllFiguresAsync(string userId, int pageNumber, int figuresPerPage)
        {
            var figures = await this._dbContext.UserFigures
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Figure.RetailPrices)
                .Include(uf => uf.Figure.CurrentAftermarketPrices)
                .Include(uf => uf.Figure.AftermarketPrices)
                .Select(f => f.Figure)
                .Skip((pageNumber - 1) * figuresPerPage)
                .Take(figuresPerPage)
                .ToListAsync();

            return figures.Select(f => new FigureInListViewModel
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = f.Image,
                RetailPrice = f.RetailPrices?
                    .OrderByDescending(rp => rp.ReleaseDate)
                    .FirstOrDefault()?.Price ?? 0,
                RetailPriceCurrency = "$",
                AvgCurrentAftermarketPrice = f.CurrentAftermarketPrices != null && f.CurrentAftermarketPrices.Any()
                    ? Math.Round(f.CurrentAftermarketPrices.Average(af => af.Price), 2)
                    : (f.RetailPrices.OrderByDescending(rp => rp.ReleaseDate).FirstOrDefault()?.Price ?? 0),
                AvgAftermarketPriceCurrency = "$",
                AftermarketPrices = f.AftermarketPrices
            });
        }

        public async Task UpdateUserFigureCollectionUrlAsync(string userId, string url)
        {
            var user = await this._dbContext.UserFigureCollectionUrls.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user != null)
            {
                user.FigureCollectionUrl = url;
                await this._dbContext.SaveChangesAsync();
            }
            else
            {
                var userFigureCollectionUrl = new UserFigureCollectionUrl
                {
                    UserId = userId,
                    FigureCollectionUrl = url
                };

                await this._dbContext.UserFigureCollectionUrls.AddAsync(userFigureCollectionUrl);
                await this._dbContext.SaveChangesAsync();
            }
        }

        public async Task<string> GetUserFigureCollectionUrlAsync(string userId)
        {
            var userFigureCollectionUrl = await this._dbContext.UserFigureCollectionUrls.FirstOrDefaultAsync(u => u.UserId == userId);
            return userFigureCollectionUrl?.FigureCollectionUrl;
        }

        public async Task<decimal> SumRetailPriceCollectionAsync(string userId)
        {
            var figures = await this._dbContext.UserFigures
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Figure.RetailPrices)
                .Select(f => f.Figure)
                .ToListAsync();

            return figures.Select(f => f.RetailPrices
                          .OrderByDescending(rp => rp.ReleaseDate)
                          .FirstOrDefault()?.Price ?? 0).Sum();

        }

        public async Task<decimal> SumAvgAftermarketPriceCollectionAsync(string userId)
        {
            var figures = await this._dbContext.UserFigures
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Figure.RetailPrices)
                .Include(uf => uf.Figure.AftermarketPrices)
                .Select(f => f.Figure)
                .ToListAsync();

            return figures.Select((f =>
                f.AftermarketPrices != null && f.AftermarketPrices.Any()
                    ? Math.Round(f.AftermarketPrices.Average(af => af.Price), 2)
                    : f.RetailPrices.OrderByDescending(rp => rp.ReleaseDate).FirstOrDefault()?.Price ?? 0)
            ).Sum();
        }

        public async Task<Dictionary<string, int>> GetFigureUrlsWithOutdatedAftermarketPricesAsync()
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-1);

            return await this._dbContext.Figures
                .Where(f => (f.LastUpdatedRetailPrices <= thresholdDate))
                .ToDictionaryAsync(f => f.FigureUrl, f => f.Id);
        }
    }
}
