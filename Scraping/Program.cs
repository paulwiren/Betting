using PuppeteerSharp;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
         var connectionString = "FootballStatsApiRedisCache.redis.cache.windows.net:6380,password=UYxwXs7y498EsBI82A8X2ZnDPau7okYOwAzCaC6CJqQ=,ssl=True,abortConnect=False";
        // Replace with your Azure Redis connection string

        var options = ConfigurationOptions.Parse(connectionString);
        options.ConnectTimeout = 10000; // 10 seconds
        options.SyncTimeout = 10000;
        try
        {
            // Connect to Azure Redis Cache
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);
            IDatabase db = redis.GetDatabase();

            // Set and Get example
            string key = "testKey";
            string value = "Hello, Azure Redis!";

            db.StringSet(key, value);
            string retrievedValue = db.StringGet(key);

            Console.WriteLine($"Stored: {value}");
            Console.WriteLine($"Retrieved: {retrievedValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        //await DownloadBrowser();
    }


    private static async Task DownloadBrowser()
    {
        await new BrowserFetcher().DownloadAsync();
        var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        //x-mas: eyJib2R5Ijp7InVybCI6Ii9hcGkvbWF0Y2hlcz9kYXRlPTIwMjUwMjEyIiwiY29kZSI6MTczOTQwMDEzMjIyMywiZm9vIjoicHJvZHVjdGlvbjo2Mjk5ZGRmMTAwMjU3YTg2Y2QzYWY2Y2VmOWNjYWVhMzQ1MTA5YTdhLXVuZGVmaW5lZCJ9LCJzaWduYXR1cmUiOiJCRkNCQ0Y3NkNERDBBQjMwMjVEMzZFMjgxNEM4NTE5QSJ9
        page.Request += (sender, e) =>
        {
            Console.WriteLine($"URL: {e.Request.Url}");
            Console.WriteLine("Headers:");
            foreach (var header in e.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }
        };

        await page.GoToAsync("https://fotmob.com/sv");
        await browser.CloseAsync();
    }

}
//// See https://aka.ms/new-console-template for more information
//using HtmlAgilityPack;
//using Scraping;
//using System.Text.Json;
//using System.Text.RegularExpressions;

////string url = "https://spela.svenskaspel.se/stryktipset";
////string url = "https://spela.svenskaspel.se/europatipset";
//string url = "https://spela.svenskaspel.se/topptipset";

//var htmlContent = await GetAPI(url);

////var htmlContent = await ReadFromFile(url);
//string json = await GetScript(htmlContent);

//var ids = await GetSportRadarIds(json);

//string baseUrl = "https://widgets.fn.sportradar.com/svenskaspel/se/Etc:UTC/gismo/match_info/";

//var matches = new List<Scraping.Match>();
//var settings = new JsonSerializerOptions
//{
//    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
//};
//foreach (var id in ids)
//{
//    var res = await GetAPI(baseUrl + id);
//    if (string.IsNullOrEmpty(res)) continue;
//    var game = JsonSerializer.Deserialize<Game>(res, settings);
//    var home = game.Doc.First().Data.Match.Teams.Home;
//    var away = game.Doc.First().Data.Match.Teams.Away;
//    matches.Add(game.Doc.First().Data.Match);   
//}

//string baseStatsTeamUrl = "https://widgets.fn.sportradar.com/svenskaspel/se/Etc:UTC/gismo/stats_team_lastxextended/";

//var games = new List<GameHistory>();
//// foreach game
//foreach(var m in matches)
//{
//    int homeId = m.Teams.Home.Id;
//    int awayId = m.Teams.Away.Id;

//    var homeTeamLastMathes = await GetAPI(baseStatsTeamUrl + homeId);
//    var awayTeamLastMatches = await GetAPI(baseStatsTeamUrl + awayId);
//    var homeStats = JsonSerializer.Deserialize<Game>(homeTeamLastMathes);
//    var awayStats = JsonSerializer.Deserialize<Game>(homeTeamLastMathes);
//    var home = new Team { Id = homeId, Name = m.Teams.Home.Name, Matches = homeStats.Doc.First().Data.Matches.Take(5).Select(x => GetHistoryMatch(homeId, x)).ToList()};
//    var away = new Team { Id = awayId, Name = m.Teams.Away.Name, Matches = awayStats.Doc.First().Data.Matches.Take(5).Select(x => GetHistoryMatch(awayId, x)).ToList()};
//    home.Stats.Sum = home.Matches.Sum(z => z.Score.Points);
//    away.Stats.Sum = away.Matches.Sum(z => z.Score.Points);
//    games.Add(new GameHistory {Home = home, Away = away });

//    Console.WriteLine($"{home.Name} ({home.Stats.Sum}) - {away.Name} ({away.Stats.Sum})");

//    //break;
//}


// Scraping.Match GetHistoryMatch(int currentTeamId, Scraping.Match match)
//{
//    var extendedMatch = match;
//    var teamType = "home";

//    if(match.Teams.Away.Id == currentTeamId)
//    {
//        teamType = "away";
//    }

//    // Eval result winner,draw,lost 3,1,0
//    var winner = match.Score.Winner;
//    if(winner == null)
//    {
//        extendedMatch.Score.Points = 1;
//    }
//    else if (winner.Equals(teamType))
//    {        
//        extendedMatch.Score.Points = 3;
//        if (teamType.Equals("away"))
//        {
//            extendedMatch.Score.Points += 1;
//        }
//    }

//    return extendedMatch;

//}

//async Task<List<string>> GetSportRadarIds(string json)
//{
//    var ids = new List<string>();
//    //string pattern = @"sportradarId:"
//    string pattern = @"""sportradarId"":""([^""]+)""";

//    // Create a Regex
//    Regex rg = new Regex(pattern);

//    // Get all matches
//    MatchCollection matchedAuthors = rg.Matches(json);
//    foreach (System.Text.RegularExpressions.Match match in matchedAuthors)
//    {
//        string id = match.Value.Replace("sportradarId","").Replace("\"","").Replace(":","");
//        ids.Add(id);
//    }
//    return ids.Distinct().ToList();
//    //return matchedAuthors.FirstOrDefault().Value;
//}
// async Task<string> GetScript(string htmlContent)
//    {
//    // Load HTML content into HtmlDocument
//    HtmlDocument htmlDocument = new HtmlDocument();
//    htmlDocument.LoadHtml(htmlContent);

//    // Find script tags
//    var scriptTags = htmlDocument.DocumentNode.Descendants("script");
//    string targetRow = "svs.tipsen.data.preloadedState=";
//    bool cancel = false;
//    string script = string.Empty;
//    // Print the content of each script tag
//    foreach (var scriptTag in scriptTags)
//    {
//        string scriptContent = scriptTag.InnerHtml;

//        // Split the script content into lines
//        string[] scriptLines = scriptContent.Split('\n');

//        // Find and print a specific row (e.g., the line containing "var x = 42;")
//        foreach (string line in scriptLines)
//        {
//            if (line.Contains(targetRow))
//            {
//                script = line;
//                cancel = true;
//            }
//        }
//        if (cancel) { break; }
//    }

//    // Find jsonstring
//    string json = script.Trim().Substring(targetRow.Length+1);
//    json = json.Replace(";", "" );
//    return json;

//}


//async Task<string>  GetAPI(string url)
//{
//    // Skapa en HttpClient
//    using (HttpClient httpClient = new HttpClient())
//    {
//        try
//        {
//            // Hämta webbsidan som en sträng
//            return await httpClient.GetStringAsync(url);           
//        }
//        catch (HttpRequestException e)
//        {
//            Console.WriteLine($"Error: {e.Message}");
//        }
//    }
//    return string.Empty;
//}

//async Task<string> ReadFromFile(string url)
//{
//    string filePath = @"C:\Users\Paul\Documents\europatips.html";
//    try
//    {
//        return File.ReadAllText(filePath);       
//    }
//    catch (IOException e)
//    {
//        Console.WriteLine("Det uppstod ett fel vid läsning av filen: " + e.Message);
//    }
//    return string.Empty;
//}


