using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using Microsoft.Extensions.Options;
using MyFigureCollectionValue.Models;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace MyFigureCollectionValue.Services
{
    public class ScraperService : IScraperService
    {
        private IBrowsingContext _context;
        private readonly IFigureService _figureService;
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;
        private readonly ScraperSettings _settings;

        public ScraperService(IBrowsingContext context, IOptions<ScraperSettings> settings, IFigureService figureService)
        {
            this._context = context;
            this._figureService = figureService;
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

                var ownedElement = initialDocument.QuerySelectorAll("nav.actions a")
                                                  .FirstOrDefault(a => a.TextContent.Contains("Owned"));

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

                        var itemIds = currentPageElement.QuerySelectorAll("div.item-icons span.item-icon a")
                                                        .Select(s => s.Attributes["href"].Value)
                                                        .ToList();

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

        public async Task<(ICollection<Figure> Figures, ICollection<RetailPrice> RetailPrices, ICollection<AftermarketPrice> AftermarketPrices)>
            CreateFiguresAndPricesAsync(IEnumerable<string> figureUrls, string userId)
        {
            var newFigureList = new List<Figure>();
            var retailPriceList = new List<RetailPrice>();
            var aftermarketPriceList = new List<AftermarketPrice>();
            var delayRequest = new Random();
            const int maxRetries = 5;

            try
            {
                await this._figureService.RemoveUserFiguresAsync(userId);

                foreach (var url in figureUrls)
                {
                    int figureId = int.Parse(url.Split("/item/")[1]);

                    if (await this._figureService.DoesFigureExistAsync(figureId))
                    {
                        await this._figureService.AddExistingFigureToUserAsync(figureId, userId);
                        continue;
                    }

                    int retries = 0;
                    bool success = false;

                    while (!success && retries < maxRetries)
                    {
                        try
                        {
                            var requestDelay = delayRequest.Next(500, 800);
                            await Task.Delay(requestDelay);

                            var document = await this._context.OpenAsync(url);

                            string name = document.QuerySelector("h1.title")?.TextContent?.Trim()
                                          ?? throw new NullReferenceException("Title not found");

                            var originElement = document.QuerySelectorAll("div.data-label")
                                                        .FirstOrDefault(x => x.TextContent.Contains("Origin"));

                            string origin = originElement?.NextElementSibling?.QuerySelector("span")?.TextContent?.Trim()
                                            ?? "Unknown Origin";

                            var companiesElement = document.QuerySelectorAll("div.data-label")
                                                           .FirstOrDefault(x => x.TextContent.Contains("Companies")
                                                           || x.TextContent.Contains("Company"));

                            string companies = string.Join(", ",
                                companiesElement.NextElementSibling.QuerySelectorAll("span")
                                                .Zip(companiesElement.NextElementSibling.QuerySelectorAll("small"),
                                                    (span, small) => $"{span.TextContent} {small.TextContent}"));

                            string imageUrl = document.QuerySelector("a.main img").GetAttribute("src");
                            string updatedUrl = imageUrl.Replace("/items/1/", "/items/2/");
                            var response = await this._client.SendAsync(new HttpRequestMessage(System.Net.Http.HttpMethod.Head, updatedUrl));

                            if (!response.IsSuccessStatusCode)
                            {
                                updatedUrl = imageUrl;
                            }

                            var newFigure = new Figure
                            {
                                Id = figureId,
                                Name = name,
                                Origin = origin,
                                Company = companies,
                                Image = updatedUrl,
                                FigureUrl = url,
                                LastUpdated = DateTime.UtcNow,
                            };

                            newFigureList.Add(newFigure);

                            var figureRetailPrice = await GetRetailPriceListAsync(url, figureId);

                            if (figureRetailPrice != null)
                            {
                                retailPriceList.AddRange(figureRetailPrice);
                            }

                            var figureAftermarketPrice = await GetAftermarketPriceListAsync(url, figureId);

                            if (figureAftermarketPrice != null)
                            {
                                aftermarketPriceList.AddRange(figureAftermarketPrice);
                            }

                            success = true;
                        }
                        catch (NullReferenceException ex)
                        {
                            retries++;
                            Console.WriteLine($"Attempt {retries} failed for URL: {url}. Error: {ex.Message}");

                            if (retries >= maxRetries)
                            {
                                Console.WriteLine("Max retries reached. Stopping execution.");
                                break;
                            }

                            await Task.Delay(2000);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"An error occurred: {ex.Message}");
                        }
                    }

                    if (retries >= maxRetries)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return (newFigureList, retailPriceList, aftermarketPriceList);
        }


        public async Task<ICollection<RetailPrice>> GetRetailPriceListAsync(string url, int figureId)
        {
            var retailPriceList = new List<RetailPrice>();

            var document = await this._context.OpenAsync(url);

            var dataField = document.QuerySelectorAll("div.data-field")
                                    .FirstOrDefault(df => df.FirstChild.TextContent.Contains("Releases"));

            if (dataField != null)
            {
                var label = dataField.QuerySelector("div.data-label");

                if (label != null && label.TextContent.Contains("Releases"))
                {
                    var retialPrice = await ExtractRetailPriceAsync(dataField, figureId);

                    if (retialPrice != null)
                    {
                        retailPriceList.Add(retialPrice);
                    }
                    else
                    {
                        return null;
                    }

                    var nextSibling = dataField.NextElementSibling;

                    while (nextSibling != null)
                    {
                        var labelElement = nextSibling.QuerySelector("div.data-label");

                        if (labelElement == null)
                        {
                            var nextRetailPrice = await ExtractRetailPriceAsync(nextSibling, figureId);

                            if (nextRetailPrice != null)
                            {
                                retailPriceList.Add(nextRetailPrice);
                                nextSibling = nextSibling.NextElementSibling;
                            }
                        }
                        else
                        {
                            break;
                        }

                    }
                }

                return retailPriceList;
            }
            else
            {
                return null;
            }
        }

        private async Task<RetailPrice> ExtractRetailPriceAsync(AngleSharp.Dom.IElement dataField, int figureId)
        {
            var dataValue = dataField.QuerySelector("div.data-value");

            var dateElement = dataValue.QuerySelector("a.time");

            DateTime releaseDate;

            string dateText = dateElement.TextContent.Trim();

            if (dateText.Length == 10)
            {
                releaseDate = DateTime.ParseExact(dateText, "MM/dd/yyyy", null);
            }
            else if (dateText.Length == 7)
            {
                releaseDate = DateTime.ParseExact($"01/{dateText}", "dd/MM/yyyy", null);
            }
            else if (dateText.Length == 4 && int.TryParse(dateText, out int year))
            {
                releaseDate = new DateTime(year, 1, 1);
            }
            else
            {
                throw new FormatException($"Unexpected date format: {dateText}, figureId: {figureId}");
            }

            string dataText = dataValue.InnerHtml;

            var priceTextSection = dataText.Split("<br>").Skip(1).FirstOrDefault()?.Trim();

            if (priceTextSection == null)
            {
                return null;
            }

            var priceText = priceTextSection.Split(" ").FirstOrDefault()?.Trim().Replace(",", "");

            if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                Console.WriteLine($"Failed to parse price from '{priceText}', figureId: {figureId}");

                return null;
            }

            var smallElements = dataValue.QuerySelectorAll("small");

            string currency = smallElements[1].TextContent.Split(" ")[0];

            return new RetailPrice
            {
                Price = price,
                Currency = currency,
                ReleaseDate = releaseDate,
                FigureId = figureId,
            };
        }

        public async Task<ICollection<AftermarketPrice>> GetAftermarketPriceListAsync(string url, int figureId)
        {
            using (HttpClient client = new HttpClient())
            {
                var formData = await GetFormData();
                string cookieHeader = GetCookiesForRequest(url);
                AddDefaultReuestHeaders(client, cookieHeader);

                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, formData);

                    response.EnsureSuccessStatusCode();

                    string htmlContent = await response.Content.ReadAsStringAsync();

                    var jsonDocument = JsonDocument.Parse(htmlContent);
                    string windowHtml = jsonDocument.RootElement.GetProperty("htmlValues").GetProperty("WINDOW").GetString();

                    string decodedHtml = WebUtility.HtmlDecode(windowHtml);

                    var document = await this._context.OpenAsync(req => req.Content(decodedHtml));

                    string price = document.QuerySelector("span.classified-price-value").TextContent.Trim();

                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Request error: " + e.Message);
                }
            }

            return null;
        }


        private async Task<AftermarketPrice> ExtractAftermarketPriceAsync(IElement item, int figureId)
        {

            string priceText = item.QuerySelector("span.classified-price-value").TextContent.Trim();

            if (!decimal.TryParse(priceText.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                Console.WriteLine($"Failed to parse price from '{priceText}', figureId: {figureId}");

                return null;
            }

            string currency = item.QuerySelector("span.classified-price-currency").TextContent.Trim();
            var dateElement = item.QuerySelector("span.meta[title]");
            string dateText = dateElement.GetAttribute("title");

            if (!DateTime.TryParseExact(dateText, "dd/MM/yyyy, HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime loggedAt))
            {
                Console.WriteLine($"Failed to parse date from '{dateText}', figureId: {figureId}");
                return null;
            }

            return new AftermarketPrice
            {
                Price = price,
                Currency = currency,
                LoggedAt = loggedAt,
                FigureId = figureId,
            };
        }

        private async Task<FormUrlEncodedContent> GetFormData()
        {
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("commit", "loadWindow"),
                new KeyValuePair<string, string>("window", "buyItem"),
                new KeyValuePair<string, string>("soldBy", "users"),
                new KeyValuePair<string, string>("jan", "4560228202793"),
            });
        }

        private void AddDefaultReuestHeaders(HttpClient client, string cookieHeader)
        {
            client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            client.DefaultRequestHeaders.Add("Referer", "https://myfigurecollection.net/");
        }

        private string GetCookiesForRequest(string url)
        {
            var uri = new Uri(url);
            var cookies = _cookieContainer.GetCookies(uri).Cast<Cookie>();

            return string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));
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