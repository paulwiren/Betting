using BettingEngine.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BettingEngine.Services
{
    public interface ICouponService
    {
        Task<string> GetCouponAsync(string coupong);
       
    }
    public class CouponService : ICouponService
    {
        public HttpClient _httpClient;
        public CouponService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> GetCouponAsync(string coupong)
        {
            try
            {
                var htmlCoupong = await _httpClient.GetStringAsync(coupong);
                return await GetCouponJson(htmlCoupong);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            return string.Empty;
        }

        public async Task<BettingBoard> GetCouponAsync(string jsonCoupon, int numberOfMatches = 13)
        {
            var ids = await GetSportRadarIds(jsonCoupon);

            var dates = await GetCloseDates(jsonCoupon);
            // Only take matches for today or tomorrow - datecheck
            int numberOfCoupons = 1;
            if (numberOfMatches < 13)
            {
                numberOfCoupons = dates.Where(d => d.Date.Date <= DateTime.Now.AddDays(1).Date).Count();
            }

            //var matches = await GetMatches(ids.Take(numberOfCoupons * numberOfMatches));
            return await CreateBettingBoard(numberOfMatches, dates.ToList());
        }

        private async Task<BettingBoard> CreateBettingBoard(int numberOfMatches, List<DateTime> dates)
        {
            var games = new List<GameHistory>();
            games.Capacity = numberOfMatches;
           
                // Get fotmob data
                //games.Add(new GameHistory { Home = home, Away = away });
            

            var board = new BettingBoard();
            Coupon coupon = new Coupon();
            coupon.GameStop = dates.First();
            int dateIndex = 1;
            for (int i = 0; i < games.Count; i++)
            {
                coupon.Games.Add(games[i]);

                // Only topptips
                if (numberOfMatches < 13 && (i + 1) % numberOfMatches == 0)
                {
                    board.Coupons.Add(coupon);
                    coupon = new Coupon();
                    coupon.GameStop = dates[dateIndex];
                    dateIndex++;
                }
            }
            if (numberOfMatches >= 13)
            {
                board.Coupons.Add(coupon);
            }
            return board;
        }

        private async Task<IEnumerable<DateTime>> GetCloseDates(string json)
        {
            string pattern = @"""regCloseTime"":""(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{2}:\d{2})""";

            // Create a Regex
            Regex rg = new Regex(pattern);

            // Get all matches
            MatchCollection matchedAuthors = rg.Matches(json);

            //pattern = @"""regCloseTime"":""(\d{4}""";
            //rg = new Regex(pattern);
            matchedAuthors = rg.Matches(json);

            var dates = new List<DateTime>();
            foreach (System.Text.RegularExpressions.Match match in matchedAuthors)
            {
                string date = match.Value.Replace("regCloseTime", "").Replace("\"", "");
                date = date.Substring(1, date.Length - 1);
                dates.Add(DateTime.Parse(date));
            }
            return dates.ToList();
        }
        private async Task<List<string>> GetSportRadarIds(string json)
        {
            var ids = new List<string>();
            //string pattern = @"sportradarId:"
            string pattern = @"""sportradarId"":""([^""]+)""";

            // Create a Regex
            Regex rg = new Regex(pattern);

            // Get all matches
            MatchCollection matchedAuthors = rg.Matches(json);
            foreach (System.Text.RegularExpressions.Match match in matchedAuthors)
            {
                string id = match.Value.Replace("sportradarId", "").Replace("\"", "").Replace(":", "");
                ids.Add(id);
            }
            return ids.Distinct().ToList();
        }

        private async Task<string> GetCouponJson(string htmlContent)
        {
            // Load HTML content into HtmlDocument
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // Find script tags
            var scriptTags = htmlDocument.DocumentNode.Descendants("script");
            string targetRow = "svs.tipsen.data.preloadedState=";
            bool cancel = false;
            string script = string.Empty;
            // Print the content of each script tag
            foreach (var scriptTag in scriptTags)
            {
                string scriptContent = scriptTag.InnerHtml;

                // Split the script content into lines
                string[] scriptLines = scriptContent.Split('\n');

                // Find and print a specific row (e.g., the line containing "var x = 42;")
                foreach (string line in scriptLines)
                {
                    if (line.Contains(targetRow))
                    {
                        script = line;
                        cancel = true;
                    }
                }
                if (cancel) { break; }
            }

            // Find jsonstring
            string json = script.Trim().Substring(targetRow.Length + 1);
            return json.Replace(";", "");
        }

    }
}
