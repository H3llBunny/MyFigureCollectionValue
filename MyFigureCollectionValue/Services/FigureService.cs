using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MyFigureCollectionValue.Data;
using MyFigureCollectionValue.Models;
using System.Linq;

namespace MyFigureCollectionValue.Services
{
    public class FigureService : IFigureService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrencyConverterService _currencyConverter;
        private const string DefaultCurrencySymbol = "$";

        public FigureService(ApplicationDbContext dbContext, ICurrencyConverterService currencyConverter)
        {
            _dbContext = dbContext;
            this._currencyConverter = currencyConverter;
        }

        public async Task<bool> DoesFigureExistAsync(int id)
        {
            return await _dbContext.Figures.AnyAsync(f => f.Id == id);
        }

        public async Task AddFiguresAsync(IEnumerable<Figure> figures)
        {
            await _dbContext.AddRangeAsync(figures);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddRetailPricesAsync(IEnumerable<RetailPrice> retailPrices)
        {
            await _dbContext.AddRangeAsync(retailPrices);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddCurrentAftermarketPricesAsync(IEnumerable<CurrentAftermarketPrice> currentAftermarketPrices)
        {
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM CurrentAftermarketPrices");

            await _dbContext.AddRangeAsync(currentAftermarketPrices);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddAftermarketPricesAsync(IEnumerable<AftermarketPrice> aftermarketPrices)
        {
            var priceIds = aftermarketPrices.Select(ap => ap.Id).ToList();

            var existingPriceIds = await _dbContext.AftermarketPrices
                .Where(ap => priceIds.Contains(ap.Id))
                .Select(ap => ap.Id)
                .ToListAsync();

            var newPrices = aftermarketPrices.Where(ap => !existingPriceIds.Contains(ap.Id)).ToList();

            if (newPrices.Any())
            {
                await _dbContext.AftermarketPrices.AddRangeAsync(newPrices);
                await _dbContext.SaveChangesAsync();
            }

            var existingAds = await _dbContext.AftermarketPrices.Where(ap => existingPriceIds.Contains(ap.Id)).ToListAsync();

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
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task AddUserFiguresAsync(string userId, IEnumerable<Figure> figureList)
        {
            var userFigures = figureList.Select(figure => new UserFigure
            {
                UserId = userId,
                FigureId = figure.Id,
            }).ToList();

            await _dbContext.UserFigures.AddRangeAsync(userFigures);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveUserFiguresAsync(string userId)
        {
            var userFigures = _dbContext.UserFigures.Where(uf => uf.UserId == userId);

            _dbContext.UserFigures.RemoveRange(userFigures);

            await _dbContext.SaveChangesAsync();
        }

        public async Task AddExistingFigureToUserAsync(int figureId, string userId)
        {
            var userFigure = new UserFigure
            {
                UserId = userId,
                FigureId = figureId
            };

            await _dbContext.UserFigures.AddAsync(userFigure);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetUserFiguresCountAsync(string userId)
        {
            return await _dbContext.UserFigures.Where(u => u.UserId == userId).CountAsync();
        }

        public async Task<IEnumerable<FigureInListViewModel>> GetAllFiguresAsync(string userId, int pageNumber, int figuresPerPage, string sortOrder)
        {
            var query = _dbContext.UserFigures
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Figure.RetailPrices)
                .Include(uf => uf.Figure.CurrentAftermarketPrices)
                .Include(uf => uf.Figure.AftermarketPrices)
                .Select(f => f.Figure)
                .AsNoTracking();

            query = sortOrder switch
            {
                "retail_asc" => query.OrderBy(f => f.RetailPrices.Any() 
                ? f.RetailPrices.OrderByDescending(rp => rp.ReleaseDate).FirstOrDefault().Price
                : 0),
                "retail_desc" => query.OrderByDescending(f => f.RetailPrices.Any()
                ? f.RetailPrices.OrderByDescending(rp => rp.ReleaseDate).FirstOrDefault().Price
                : 0),
                "am_asc" => query.OrderBy(f => f.CurrentAftermarketPrices.Any() 
                ? f.CurrentAftermarketPrices.Average(cap => cap.Price)
                : 0),
                "am_desc" => query.OrderByDescending(f => f.CurrentAftermarketPrices.Any()
                ? f.CurrentAftermarketPrices.Average(cap => cap.Price)
                : 0),
                 _ => query
            };

            var figures = await query
                .Skip((pageNumber - 1) * figuresPerPage)
                .Take(figuresPerPage)
                .ToListAsync();

            if (figures.Any())
            {
                var retailPriceTask = _currencyConverter.ConvertRetailPricesToUSDAsync(figures.SelectMany(f => f.RetailPrices).ToList());
                var aftermarketPriceTask = _currencyConverter.ConvertAftermarketPricesToUSDAsync(figures.SelectMany(f => f.CurrentAftermarketPrices).ToList());
                await Task.WhenAll(retailPriceTask, aftermarketPriceTask);
            }

            return figures.Select(f => new FigureInListViewModel
            {
                Id = f.Id,
                Name = f.Name,
                ImageUrl = f.Image,
                RetailPrice = f.RetailPrices?
                    .OrderByDescending(rp => rp.ReleaseDate)
                    .FirstOrDefault()?.Price ?? 0,
                RetailPriceCurrency = DefaultCurrencySymbol,
                AvgCurrentAftermarketPrice = f.CurrentAftermarketPrices != null && f.CurrentAftermarketPrices.Any()
                    ? Math.Round(f.CurrentAftermarketPrices.Average(af => af.Price), 2)
                    : (f.RetailPrices.OrderByDescending(rp => rp.ReleaseDate).FirstOrDefault()?.Price ?? 0),
                AvgAftermarketPriceCurrency = DefaultCurrencySymbol,
                AftermarketPrices = f.AftermarketPrices
            });
        }

        public async Task UpdateUserFigureCollectionUrlAsync(string userId, string url)
        {
            var user = await _dbContext.UserFigureCollectionUrls.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user != null)
            {
                user.FigureCollectionUrl = url;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                var userFigureCollectionUrl = new UserFigureCollectionUrl
                {
                    UserId = userId,
                    FigureCollectionUrl = url
                };

                await _dbContext.UserFigureCollectionUrls.AddAsync(userFigureCollectionUrl);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<string> GetUserFigureCollectionUrlAsync(string userId)
        {
            var userFigureCollectionUrl = await _dbContext.UserFigureCollectionUrls.FirstOrDefaultAsync(u => u.UserId == userId);
            return userFigureCollectionUrl?.FigureCollectionUrl;
        }

        public async Task<decimal> SumRetailPriceCollectionAsync(string userId)
        {
            var figures = await _dbContext.UserFigures
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Figure.RetailPrices)
                .Select(f => f.Figure)
                .ToListAsync();

            if (!figures.Any())
            {
                return 0;
            }

            await _currencyConverter.ConvertRetailPricesToUSDAsync(figures.SelectMany(f => f.RetailPrices).ToList());

            return figures.Select(f => f.RetailPrices
                          .OrderByDescending(rp => rp.ReleaseDate)
                          .FirstOrDefault()?.Price ?? 0).Sum();

        }

        public async Task<decimal> SumAvgAftermarketPriceCollectionAsync(string userId)
        {
            var figures = await _dbContext.UserFigures
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Figure.RetailPrices)
                .Include(uf => uf.Figure.CurrentAftermarketPrices)
                .Select(f => f.Figure)
                .ToListAsync();

            if (!figures.Any())
            {
                return 0;
            }

            await _currencyConverter.ConvertAftermarketPricesToUSDAsync(figures.SelectMany(f => f.CurrentAftermarketPrices).ToList());

            return figures.Select((f =>
                f.CurrentAftermarketPrices != null && f.CurrentAftermarketPrices.Any()
                    ? Math.Round(f.CurrentAftermarketPrices.Average(af => af.Price), 2)
                    : f.RetailPrices.OrderByDescending(rp => rp.ReleaseDate).FirstOrDefault()?.Price ?? 0)
            ).Sum();
        }

        public async Task<Dictionary<string, int>> GetFigureUrlsWithOutdatedAftermarketPricesAsync()
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-1);

            return await _dbContext.Figures
                .Where(f => (f.LastUpdatedRetailPrices <= thresholdDate))
                .ToDictionaryAsync(f => f.FigureUrl, f => f.Id);
        }

        public async Task UpdateFiguresLastUpdatedRetailPricesAsync(List<int> figureIds)
        {
            var parameters = figureIds.Select((figureId, index) => new SqlParameter($"@id{index}", figureId)).ToList();
            var sqlParameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));

            await _dbContext.Database.ExecuteSqlRawAsync($@"
                UPDATE Figures
                SET LastUpdatedRetailPrices = @currentDate
                WHERE Id IN ({sqlParameterNames});
            ", parameters.Append(new SqlParameter("@currentDate", DateTime.UtcNow)).ToArray());
        }

        public async Task<ICollection<string>> GetOutdatedFigureUrlsAsync()
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-7);

            return await _dbContext.Figures
                .Where(f => (f.LastUpdated <= thresholdDate))
                .Select(f => f.FigureUrl)
                .ToListAsync();
        }

        public async Task UpdateFiguresAsync(ICollection<Figure> figures)
        {
            var figureIds = figures.Select(f => f.Id).ToList();

            var existingFigures = await _dbContext.Figures
                .Where(f => figureIds.Contains(f.Id))
                .ToListAsync();

            foreach (var existingFigure in existingFigures)
            {
                var updatedFigure = figures.First(f => f.Id == existingFigure.Id);

                existingFigure.Name = updatedFigure.Name;
                existingFigure.Origin = updatedFigure.Origin;
                existingFigure.Company = updatedFigure.Company;
                existingFigure.Image = updatedFigure.Image;
                existingFigure.LastUpdated = updatedFigure.LastUpdated;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRetailPricesAsync(ICollection<RetailPrice> retailPrices)
        {
            var figureIds = retailPrices.Select(rp => rp.FigureId).Distinct().ToList();

            var existingRetailPrices = await _dbContext.RetailPrices
                .Where(rp => figureIds.Contains(rp.FigureId))
                .ToListAsync();

            var newRetailPrices = retailPrices
                .Where(newPrice =>
                    !existingRetailPrices.Any(existingPrice =>
                        existingPrice.FigureId == newPrice.FigureId &&
                        existingPrice.ReleaseDate == newPrice.ReleaseDate))
                .ToList();

            if (newRetailPrices.Any())
            {
                await _dbContext.RetailPrices.AddRangeAsync(newRetailPrices);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<FigurePageViewModel> GetFigureAsync(int figureId)
        {
            var figure = await _dbContext.Figures
                .Where(f => f.Id == figureId)
                .Include(f => f.RetailPrices)
                .Include(f => f.CurrentAftermarketPrices)
                .Include(f => f.AftermarketPrices)
                .FirstOrDefaultAsync();

            if (figure != null)
            {
                var retailPriceTask = _currencyConverter.ConvertRetailPricesToUSDAsync(figure.RetailPrices);
                var currentAftermarketPriceTask = _currencyConverter.ConvertAftermarketPricesToUSDAsync(figure.CurrentAftermarketPrices);
                var aftermarketPriceTask = _currencyConverter.ConvertAftermarketPricesToUSDAsync(figure.AftermarketPrices);
                await Task.WhenAll(retailPriceTask, aftermarketPriceTask);
            }

            return new FigurePageViewModel
            {
                Id = figure.Id,
                Name = figure.Name,
                ImageUrl = figure.Image,
                Origin = figure.Origin,
                Company = figure.Company,
                FigureUrl = figure.FigureUrl,
                RetailPrice = figure.RetailPrices?
                   .OrderByDescending(rp => rp.ReleaseDate)
                   .FirstOrDefault()?.Price ?? 0,
                RetailPriceCurrency = DefaultCurrencySymbol,
                AvgCurrentAftermarketPrice = figure.CurrentAftermarketPrices != null && figure.CurrentAftermarketPrices.Any()
                   ? Math.Round(figure.CurrentAftermarketPrices.Average(af => af.Price), 2)
                   : (figure.RetailPrices.OrderByDescending(rp => rp.ReleaseDate).FirstOrDefault()?.Price ?? 0),
                AvgAftermarketPriceCurrency = DefaultCurrencySymbol,
                AftermarketPrices = figure.AftermarketPrices
            };
        }
    }
}
