using BettingEngine.Models;
using PuppeteerSharp;

namespace BettingEngine.Services
{
    public class PuppeteerService
    {
        public async Task<List<string>> GetSubRequestHeadersAsync(string url)
        {
            await new BrowserFetcher().DownloadAsync(BrowserTag.Stable);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            var headersList = new List<string>();
            string header = string.Empty;
            page.Request += (sender, e) =>
            {
                //e.Request.Url
                // https://www.fotmob.com/api/matches
                if (e.Request.Url.Contains("https://www.fotmob.com/api/matches"))
                {
                    var headers = e.Request.Headers.Where(h => h.Key.Equals("x-mas"));
                    if (headers != null)
                    {
                        header = headers.First().Value;
                    }
                }
                //headersList.Add($"Request: {e.Request.Url}");
                //foreach (var header in e.Request.Headers)
                //{
                //    headersList.Add($"{header.Key}: {header.Value}");
                //}
            };

            await page.GoToAsync(url);
            await browser.CloseAsync();

            return headersList;
        }
    }
}
