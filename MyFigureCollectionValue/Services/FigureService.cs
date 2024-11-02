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

        public async Task AddAftermarketPricesAsync(IEnumerable<AftermarketPrice> aftermarketPrices)
        {
            await this._dbContext.AddRangeAsync(aftermarketPrices);
            await this._dbContext.SaveChangesAsync();
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

        public async Task<int> GetUserFiguresCount(string userId)
        {
            return await this._dbContext.UserFigures.Where(u => u.UserId == userId).CountAsync();
        }

        public async Task<IEnumerable<FigureInListViewModel>> GetAllFigures(string userId, int pageNumber, int figuresPerPage)
        {
            var figures = await this._dbContext.UserFigures
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Figure.RetailPrices)
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
                AvgAftermarketPrice = f.AftermarketPrices != null && f.AftermarketPrices.Any()
                    ? f.AftermarketPrices.Average(af => af.Price)
                    : 0
            });
        }
    }
}
