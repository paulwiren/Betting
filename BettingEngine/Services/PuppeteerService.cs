using BettingEngine.Models;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System.Runtime.CompilerServices;

namespace BettingEngine.Services
{
    public class PuppeteerService
    {
        private readonly ILogger<PuppeteerService> _logger;
        public PuppeteerService(ILogger<PuppeteerService> logger)
        {
            _logger = logger;
        }
        public async Task<string> GetSubRequestHeadersAsync(string url)
        {
            try
            {
                //await new BrowserFetcher().DownloadAsync(BrowserTag.Stable);
                var browseFetcher = await new BrowserFetcher().DownloadAsync();
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions { 
                                               // ExecutablePath = "/app/Chrome/Linux-132.0.6834.88/chrome-linux64/chrome", 
                                                Headless = true,
                                                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                                            });
                //var ExecutablePath = browseFetcher.GetExecutablePath(1320683488),
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

                return header;
            }
            catch(Exception ex)
            {
                var err = ex.ToString();
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }
    }
}
