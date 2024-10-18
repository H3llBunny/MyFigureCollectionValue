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
            if (retailPrices != null)
            {
                await this._dbContext.AddRangeAsync(retailPrices);
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
    }
}
