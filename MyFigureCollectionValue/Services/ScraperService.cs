using AngleSharp;
using AngleSharp.Io;
using Microsoft.Extensions.Options;
using MyFigureCollectionValue.Models;
using System.Net;

namespace MyFigureCollectionValue.Services
{
    public class ScraperService : IScraperService
    {
        private IBrowsingContext _context;
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;
        private readonly ScraperSettings _settings;

        public ScraperService(IBrowsingContext context, IOptions<ScraperSettings> settings)
        {
            this._context = context;
            this._settings = settings.Value;

            this._cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = this._cookieContainer,
                UseCookies = true,
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            this._client = new HttpClient(handler);
        }

        public async Task LoginAsync()
        {
            var loginData = new Dictionary<string, string>
            {
                { "commit", "signIn" },
                { "from", "https://myfigurecollection.net/" },
                { "username", this._settings.Username },
                { "password", this._settings.Password },
                { "remember", "1" },
                { "hide", "0" }
            };

            var content = new FormUrlEncodedContent(loginData);

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, this._settings.LoginUrl)
            {
                Content = content
            };

            request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.Add("Referer", "https://myfigurecollection.net/session/signin/");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");

            var response = await this._client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Login failed.");
            }
        }

        public async Task<IEnumerable<string>> GetAllFiguresLinkAsync(string profileUrl)
        {
            try
            {
                await SetAuthenticatedCookies(profileUrl);

                var initialDocument = await this._context.OpenAsync(profileUrl);

                if (initialDocument == null)
                {
                    throw new Exception("Failed to load the initial document. Please ensure the URL is valid and try again.");
                }
             
                var ownedElement = initialDocument.QuerySelectorAll("nav.actions a").FirstOrDefault(a => a.TextContent.Contains("Owned"));

                if (ownedElement == null)
                {
                    throw new Exception("No figures found in your collection.");
                }

                var actionCountElement = ownedElement.QuerySelector("span.action-count");

                string actionCountText = actionCountElement.TextContent.Trim();

                string figuresLink = string.Empty;

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

                await SetAuthenticatedCookies(figuresLink);
                
                var figuresDocument = await this._context.OpenAsync(figuresLink);

                if (figuresDocument == null)
                {
                    throw new Exception("Failed to load the figures document.");
                }

                var figureUrls = new List<string>();

                var pageCountElement = figuresDocument.QuerySelector("div.results-count-pages");

                if (pageCountElement != null)
                {
                    var pageLinks = pageCountElement.QuerySelectorAll("a").Select(p => p.Attributes["href"].Value);

                    foreach (var page in pageLinks)
                    {
                        var currentPageElement = await this._context.OpenAsync(page);

                        var userMenuPage = currentPageElement.QuerySelector("div.tbx-menu.user-menu");

                        var itemIds = currentPageElement.QuerySelectorAll("div.item-icons span.item-icon a").Select(s => s.Attributes["href"].Value).ToList();

                        if (itemIds == null)
                        {
                            throw new Exception("No list with figures found.");
                        }

                        figureUrls.AddRange(itemIds.Select(i => (new Uri(new Uri(profileUrl), i).ToString())));
                    }
                }
                else
                {
                    var itemIds = figuresDocument.QuerySelectorAll("span.item-icon a").Select(s => s.Attributes["href"].Value);

                    figureUrls.AddRange(itemIds.Select(i => (new Uri(new Uri(profileUrl), i).ToString())));
                }

                return figureUrls;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while scraping: {ex.Message}");
            }
        }

        private async Task SetAuthenticatedCookies(string url)
        {
            var uri = new Uri(this._settings.LoginUrl);
            var cookies = _cookieContainer.GetCookies(uri);
            var cookieHeader = string.Join("; ", cookies.Cast<Cookie>().Select(c => $"{c.Name}={c.Value}"));

            await this._context.OpenAsync(res => res.Content("<div></div>")
                .Address(url)
                .Header(HeaderNames.SetCookie, cookieHeader));
        }
    }
}