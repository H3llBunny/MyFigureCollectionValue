using AngleSharp;

namespace MyFigureCollectionValue.Services
{
    public class ScraperService : IScraperService
    {
        private readonly IBrowsingContext _context;

        public ScraperService(IBrowsingContext context)
        {
            this._context = context;
        }
        public async Task<IEnumerable<string>> GetAllItemLinksAsync(string profileUrl)
        {
            try
            {
                var initialDocument = await this._context.OpenAsync(profileUrl);

                if (initialDocument == null)
                {
                    throw new Exception("Failed to load the initial document. Please ensure the URL is valid and try again.");
                }

                var ownedElement = initialDocument.QuerySelectorAll("nav.actions a").FirstOrDefault(a => a.TextContent.Contains("Owned"));

                if (ownedElement == null)
                {
                    throw new Exception("Failed to find the action count element on the page.");
                }

                var actionCountElement = ownedElement.QuerySelector("span.action-count");

                string actionCountText = actionCountElement.TextContent.Trim();

                string figuresLink;

                if (int.TryParse(actionCountText, out int actionCount))
                {
                    if (actionCount > 0)
                    {
                        var relativeLink = ownedElement.GetAttribute("href");
                        figuresLink = new Uri(new Uri(profileUrl), relativeLink).ToString();
                    }
                    else
                    {
                        throw new Exception("No figures found in your collection.");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while scraping: {ex.Message}");
            }
        }
    }
}
