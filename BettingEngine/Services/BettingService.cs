using BettingEngine.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace BettingEngine.Services
{
    public interface IBettingService
    {
        Task<string> GetCouponAsync(string coupong);
        Task<List<string>> GetMatches(string json);

        Task<List<Percentage>> GetPercentage(string json);
    }

    public class BettingService : IBettingService
    {
        public HttpClient _httpClient;
        public BettingService(HttpClient client)
        {
            _httpClient = client;
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

        public async Task<List<string>> GetMatches(string json)
        {
            var matches = new List<string>();
            //string pattern = @"sportradarId:"
            string pattern = @"""eventDescription"":""([^""]+)""";

            // Create a Regex
            Regex rg = new Regex(pattern);

            // Get all matches
            MatchCollection matchedAuthors = rg.Matches(json);
            foreach (System.Text.RegularExpressions.Match match in matchedAuthors)
            {
                string id = match.Value.Replace("eventDescription", "").Replace("\"", "").Replace(":", "");
                matches.Add(id);
            }
            return matches.Distinct().ToList();
        }

        public async Task<List<Percentage>> GetPercentage(string json)
        {     
            try
            {
                //string data = "Global\":{\"current\":{\"value\":[\"40\", \"28\", \"32\"]";

                // Define the regex pattern for integers                
                string jsonPattern = @"Global"":\{""current"":\{""value"":\[(.*?)\]";
                string pattern = @"\d+";

                // Find matches
                MatchCollection matches = Regex.Matches(json, jsonPattern);

                MatchCollection percentagesCollection = null;

                // Convert matches to a list of strings
                List<Percentage> percentages = new List<Percentage>();
                Dictionary<int, string> counts = new Dictionary<int, string>();
                int i = 1;
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var percentage = new Percentage();
                    percentage.Id = i++;
                    percentagesCollection = Regex.Matches(match.Value, pattern);
                    percentage.Home = int.Parse(percentagesCollection[0].Value);
                    percentage.Draw = int.Parse(percentagesCollection[1].Value);
                    percentage.Away = int.Parse(percentagesCollection[2].Value);
                    percentages.Add(percentage);
                }
                return percentages;
            }
            catch(Exception ex)
            {
                var mess = ex.Message;
            }               
           
            return new List<Percentage>();
        }
    }
}
