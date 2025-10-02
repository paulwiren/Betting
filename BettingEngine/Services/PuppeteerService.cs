using Microsoft.Extensions.Logging;
using PuppeteerSharp;

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
                var chromePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH")
                 ?? "/usr/bin/chromium"; // eller "/usr/bin/chromium-browser"
                                         //var browseFetcher = await new BrowserFetcher().DownloadAsync();
                var referrer = "https://updatekeyvaultfunctionapp-7brabfqdpd5h4gr.swedencentral-01.azurewebsites.net";
                                
                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = chromePath,
                    Args = new[]
                    {
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-gpu",
                        "--single-process",
                        "--no-zygote"
                    }
                };

                var browser = await Puppeteer.LaunchAsync(launchOptions);
                var page = await browser.NewPageAsync();

                // Set only the Referer header (no policy)
               /* await page.SetExtraHttpHeadersAsync(new Dictionary<string, string> {
                                                { "Referer", referrer } // correct header name is "Referer"
                                            });*/

                var headersList = new List<string>();
                string header = string.Empty;
                page.Request += (sender, e) =>
                {
                    //e.Request.Url
                    // https://www.fotmob.com/api/matches
                    //https://www.fotmob.com/api/data/matches
                    if (e.Request.Url.Contains("https://www.fotmob.com/api/data/matches"))
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

               //await page.GoToAsync(url);
               await page.GoToAsync(url, new NavigationOptions
                {
                    Referer = referrer,   // valid
                    //Timeout = 30000,                    // valid
                   // WaitUntil = new[] { WaitUntilNavigation.Load } // valid
                });
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
