using BettingEngine.Models;
using LiveScoreBlazorApp.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using Match = BettingEngine.Models.Match;

namespace BettingEngine.Services
{
    public interface IFotMobService
    {
        //Task<List<Coupon>> GetFotMobMatches(IEnumerable<Coupon> coupons);
        Task<GameHistory> GetStats(GameHistory m);
        Task<GameHistory> GetStats(string match, DateTime date);

        Task<string> GetHeader(string findUrl, string findHeader);
    }

    public class FotMobService : IFotMobService
    {
        public HttpClient _httpClient;
        //private readonly IHttpClientFactory _httpClientFactory;
        private readonly PuppeteerService _puppeteerService;
        private readonly LevenshteinAlgorithmService _levenshteinService;
        public IMemoryCache _memoryCache;
        public JsonSerializerOptions _settings;
        public readonly TeamSynonyms _teamSynonyms;
        private readonly ILogger<FotMobService> _logger;
        private readonly IDatabase _cache;

        //https://api.spela.svenskaspel.se/multifetch?urls=/draw/1/topptipset/draws/2614
        public FotMobService(HttpClient client, IMemoryCache memoryCache, TeamSynonyms teamSynonyms, ILogger<FotMobService> logge, PuppeteerService puppeteerService, LevenshteinAlgorithmService levenshteinService, ILogger<FotMobService> logger, IDatabase cache)
        {
            _httpClient = client;
            _memoryCache = memoryCache;
            _settings = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            _teamSynonyms = teamSynonyms;
            _logger = logger;
            // _httpClientFactory = httpClientFactory;
            _puppeteerService = puppeteerService;
            _levenshteinService = levenshteinService;
            _logger = logger;
            _cache = cache;
        }

        private async Task<FotMobMatch> GetFotMobMatchData(IEnumerable<FotMobLeague> leagues, Team home, Team away)
        {
            _logger.LogWarning("GetFotMobMatchData");
            if (leagues == null)
                return null;
            var homeNames =  await _teamSynonyms.FindTeamNameSynonyms(home.Name, home.LongName);
            foreach (var l in leagues.ToList())
            {
                IEnumerable<FotMobMatch> matches = null;

                //Find home team in the day list of matches from fotmob
                foreach (var name in homeNames.ToList())
                {
                    matches = l.Matches.Where(m => m.Home.Name.ToLower().Equals(name.ToLower()));
                    if (matches != null && matches.Any())
                    {
                        break;
                    }
                }

                if (matches != null && matches.Any())
                {
                    foreach (var m in matches)
                    {
                        foreach (var name in await _teamSynonyms.FindTeamNameSynonyms(m.Away.Name, m.Away.LongName))
                        {
                            if (m.Away.Name.ToLower().Equals(name.ToLower()))
                                return m;
                        }
                    }
                }

                //// Find away team in the day list of matches from fotmob

                foreach (var name in homeNames)
                {
                    matches = l.Matches.Where(m => m.Away.Name.ToLower().Equals(name.ToLower()));
                    if (matches != null && matches.Any())
                    {
                        break;
                    }
                }

                if (matches != null && matches.Any())
                {
                    foreach (var m in matches)
                    {
                        var awayNames = await _teamSynonyms.FindTeamNameSynonyms(m.Away.Name, m.Away.LongName);
                        foreach (var name in awayNames)
                        {
                            if (m.Away.Name.ToLower().Equals(name.ToLower()))
                                return m;
                        }
                    }
                }
            }
            var fotmobMatch = _levenshteinService.FindBestMatch(home.Name, away.Name, leagues);
            if (fotmobMatch != null)
                return fotmobMatch;
            return new FotMobMatch { Id = 0, Home = new FotMobTeam { Name = home.Name }, Away = new FotMobTeam { Name = away.Name} };
        }


        private async Task<IEnumerable<FotMobLeague>> GetLatestMatchesData(Match currentMatch)
        {
            string key = currentMatch.Date.ToString("yyyy-MM-dd");
            var mathesOfTheDay = await _memoryCache.GetOrCreateAsync<IEnumerable<FotMobLeague>>(key, async cacheEntry => await GetMatches(key));           
            return mathesOfTheDay;
        }
        private async Task<IEnumerable<FotMobLeague>> GetLatestMatchesData(DateTime dateTime)
        {
            string key = dateTime.ToString("yyyy-MM-dd");
            var mathesOfTheDay = await _memoryCache.GetOrCreateAsync<IEnumerable<FotMobLeague>>(key, async cacheEntry => await GetMatches(key));
            return mathesOfTheDay;
        }

        //private async Task<FotMobMatchStats> GetMatch(int id)
        //{
        //    var result = await GetData($"matchDetails?matchId={id}");
        //    var match = JsonSerializer.Deserialize<Response>(result, _settings);
        //    if (match == null)
        //        return null;

        //    //top_stats            
        //    var stats = match.Content.Stats?.Periods.All.Stats.Where(s => s.Key.Equals("top_stats"));
        //    if (stats != null)
        //        return stats.First();

        //    return null;
        //}

        // TODO Refactoring and populate all data from match here
        private async Task<FotMobMatchStats> GetMatch(int teamId, Match m)
        {
            string cacheKey = $"fotmob-{m.Id}";
            //var result  = await _cache.StringGetAsync(cacheKey);
            //if(result.IsNull)
            //{
            //    _logger.LogInformation($"From cache - Fotmob match: {m.Id}");
            //    result = await GetData($"matchDetails?matchId={m.Id}");
            //    await _cache.StringSetAsync(cacheKey, result, TimeSpan.FromDays(120));
            //}
            var result = await GetData($"matchDetails?matchId={m.Id}");
            //var result = await GetData($"matchDetails?matchId=4506330");
            var match = JsonSerializer.Deserialize<Response>(result, _settings);
            if (match == null)
                return null;
           
            m.Score = await GetScore(teamId, match.Header.Teams[0], match.Header.Teams[1]);
           
            //top_stats            
           var stats = match.Content.Stats?.Periods.All?.Stats.Where(s => s.Key.Equals("top_stats"));
            if (stats != null)
                return stats.First();

            return null;
        }

        private async Task<Score> GetScore(int teamId, Team home, Team away)
        {
            string winner = string.Empty;            
            int points = 0;

            if(teamId == home.Id)
            {
                if(home.Score > away.Score)
                {
                    points = 3;
                    winner = "Home";
                }
                else if(home.Score < away.Score)
                {
                    points = 0;
                    winner = "Away";
                }
                else
                {
                    points = 1;
                    winner = "Draw";
                }
            }
            else if (teamId == away.Id)
            {
                if (home.Score > away.Score)
                {
                    points = 0;
                    winner = "Away";
                }
                else if (home.Score < away.Score)
                {
                    points = 3;
                    winner = "Home";
                }
                else
                {
                    points = 1;
                    winner = "Draw";
                }
            }


            return new Score {
                Home = home.Score,
                Away = away.Score,
                Winner = winner,
                Points = points
            };
        }
        //private string GetWinner(int homeScore, int awayScore)
        //{
        //    if (homeScore > awayScore)
        //        return "Home";
        //    else if (homeScore < awayScore)
        //        return "Away";
        //    return "Draw";
        //}

        private async Task<IEnumerable<FotMobLeague>> GetMatches(string date)
        {
            //https://www.fotmob.com/api/
            var mathesOfTheDay = await GetData($"matches?date={date.Replace("-","")}&timezone=Europe%2FStockholm&ccode3=SWE");
            var matches = JsonSerializer.Deserialize<FotMob>(mathesOfTheDay, _settings);
            return matches.Leagues;

        }
                
        private async Task<string> GetData(string url, bool refreshHeader = false)
        {
            try
            {
                if (refreshHeader)
                {
                    var header = await GetHeader("https://www.fotmob.com/api/matches", "x-mas");
                    _logger.LogWarning($"Response header x-mas: {header}");
                    if (!string.IsNullOrEmpty(header))
                    {
                        _httpClient.DefaultRequestHeaders.Remove("X-Mas");
                        _httpClient.DefaultRequestHeaders.Add("X-Mas", header);
                    }
                }
                
                return await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                // Try a a new request to same url with new header value
                if(!refreshHeader && ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return await GetData(url, true);
                }

                // TODO - Show error message   
                throw new HttpRequestException(ex.HttpRequestError, "No access to data");
            }
            catch (Exception ex)
            {
                throw;
            }
            //return string.Empty;
        }

        public async Task<GameHistory> GetStats(GameHistory m)
        {
            return await GetMatchData(m);
        }

        public async Task<GameHistory> GetStats(string match, DateTime date)
        {
            Team home = new Team { Name = match.Split("-")[0].Trim() };
            Team away = new Team { Name = match.Split("-")[1].Trim() };
            var listOfMatches = await GetLatestMatchesData(date);
            FotMobMatch findPlayedMatch = await GetFotMobMatchData(listOfMatches, home, away);
            if (findPlayedMatch.Id <= 0)
            {
                listOfMatches = await GetLatestMatchesData(date.AddDays(1));
                findPlayedMatch = await GetFotMobMatchData(listOfMatches, home, away);
                //if (findPlayedMatch.Id <= 0)
                //{
                //    return null;
                //}
            }
            var gameHistory = new GameHistory
            {
                Id = findPlayedMatch.Id,
                Home = new Team { Name = findPlayedMatch.Home.Name, Id = findPlayedMatch.Home.Id },
                Away = new Team { Name = findPlayedMatch.Away.Name, Id = findPlayedMatch.Away.Id }
            };

            if (gameHistory.Id > 0)
            {
                // home
                await GetTeamData(gameHistory.Home);

                // away
                await GetTeamData(gameHistory.Away);
            }

            return gameHistory;
        }
        private async Task GetTeamData(Team team)
        {

            var result = await GetData($"teams?id={team.Id}");
            //https://www.fotmob.com/api/
            //var mathesOfTheDay = await GetData($"matches?date={date.Replace("-", "")}&timezone=Europe%2FStockholm&ccode3=SWE");
            //var matches = JsonSerializer.Deserialize<FotMob>(result, _settings);
            var fixtureResponse = JsonSerializer.Deserialize<FixtureResponse>(result, _settings);
            var matches = new List<Match>();
            foreach (var match in fixtureResponse.Fixtures.AllFixtures.Fixtures)
            {
                if (match.Status.Finished)
                {
                   matches.Add(new Match
                    {
                        Id = match.Id,
                        Place = match.Home.Id == team.Id ? "home" : "away",
                        Date = match.Status.Date,
                        Teams = new Teams { Away = new Team { Id = match.Away.Id, Name = match.Away.Name }, Home = new Team { Id = match.Home.Id, Name = match.Home.Name } }
                    });
                }
            }
            team.Matches = matches.OrderByDescending(m => m.Date).Take(12).ToList();
        }

        private async Task<GameHistory> GetMatchData(GameHistory m)
        {
            if (m.Home.Matches == null)
                return m;
            int matchesAthome = m.Home.Matches.Count();
            int matchesAtAway = m.Away.Matches.Count();
            for (int i = 1; i < 13; i++)
            {
                if (i <= matchesAthome)
                {
                    var latest = m.Home.Matches[matchesAthome-i];
                    var opponent = latest.Teams.Away;
                    var home = latest.Teams.Home;
                    if (latest.Place.Equals("home"))
                    {
                        opponent = latest.Teams.Home;
                        home = latest.Teams.Away;
                        latest.IsHome = true;
                    }
                    var listOfMatches = await GetLatestMatchesData(latest);
                    // FotMobMatch findPlayedMatch = await GetFotMobMatchData(listOfMatches, m.Home, opponent);
                    FotMobMatch findPlayedMatch = await GetFotMobMatchData(listOfMatches, home, opponent);
                    if (findPlayedMatch.Id > 0)
                    {
                        latest.Id = findPlayedMatch.Id;
                        latest.FotmobUrl = $"https://fotmob.com/match/{latest.Id}";
                        FotMobMatchStats match = await GetMatch(m.Home.Id, latest);
                        if (match != null)
                        {                            
                            var totShots = match.Stats.Where(s => s.Key.Equals("total_shots")).FirstOrDefault();
                            var totShotsStat = totShots?.Stats;
                            if (totShotsStat != null && totShotsStat.Count == 2)
                            {
                                if (latest.Place.Equals("away"))
                                {
                                    m.Home.Matches[matchesAthome - i].ShotsTotal = int.Parse(totShotsStat.Last().ToString());
                                }
                                else
                                {
                                    m.Home.Matches[matchesAthome-i].ShotsTotal = int.Parse(totShotsStat.First().ToString());
                                }
                            }
                            if (m.Home.Matches[matchesAthome-i].ShotsTotal == 0)
                            {
                                var shotsOffTarget = match.Stats.Where(s => s.Key.Equals("ShotsOffTarget")).FirstOrDefault();
                                var totShotsOffTargetStat = shotsOffTarget?.Stats;
                                if (shotsOffTarget != null && totShotsOffTargetStat.Count == 2)
                                {
                                    if (latest.Place.Equals("away"))
                                    {
                                        m.Home.Matches[matchesAthome-i].ShotsTotal = int.Parse(totShotsOffTargetStat.First().ToString());
                                    }
                                    else
                                    {
                                        m.Home.Matches[matchesAthome-i].ShotsTotal = int.Parse(totShotsOffTargetStat.Last().ToString());
                                    }
                                }
                            }

                            var shotsOnTarget = match.Stats.Where(s => s.Key.Equals("ShotsOnTarget")).FirstOrDefault();
                            var shotsOnTargetStat = shotsOnTarget?.Stats;
                            if (shotsOnTargetStat != null && shotsOnTargetStat.Count == 2)
                            {
                                if (latest.Place.Equals("away"))
                                {
                                    m.Home.Matches[matchesAthome - i].ShotsOnTarget = int.Parse(shotsOnTargetStat.Last().ToString());
                                }
                                else
                                {
                                    m.Home.Matches[matchesAthome - i].ShotsOnTarget = int.Parse(shotsOnTargetStat.First().ToString());
                                }
                            }

                            var xg = match.Stats.Where(s => s.Key.Equals("expected_goals")).FirstOrDefault();
                            var xgStat = xg?.Stats;
                            if (xgStat != null && xgStat.Count == 2)
                            {
                                if (latest.Place.Equals("away"))
                                {
                                    m.Home.Matches[matchesAthome - i].ExpectedGoals = decimal.Parse(xgStat.Last().ToString());
                                }
                                else
                                {
                                    m.Home.Matches[matchesAthome - i].ExpectedGoals = decimal.Parse(xgStat.First().ToString());
                                }
                            }
                        }
                    }
                    else { Console.WriteLine("findPlayedMatch is null"); }
                }
                if (i <= matchesAtAway)
                {
                    var latest = m.Away.Matches[matchesAtAway - i];
                    var opponent = latest.Teams.Away;
                    var home = latest.Teams.Home;
                    if (latest.Place.Equals("home"))
                    {
                        opponent = latest.Teams.Home;
                        latest.IsHome = true;
                    }
                    var listOfMatches = await GetLatestMatchesData(latest);
                    var findPlayedMatch = await GetFotMobMatchData(listOfMatches, m.Away, opponent);
                    if (findPlayedMatch.Id > 0)
                    {
                        var id = findPlayedMatch.Id;
                        FotMobMatchStats match = await GetMatch(m.Away.Id, latest);
                        //var match = await GetMatch(id);
                        if (match != null)
                        {
                            latest.FotmobUrl = $"https://fotmob.com/match/{id}";
                            var totShots = match.Stats.Where(s => s.Key.Equals("total_shots")).FirstOrDefault();
                            var totShotsStat = totShots?.Stats;
                            if (totShotsStat != null && totShotsStat.Count == 2)
                            {
                                if (latest.Place.Equals("away"))
                                {
                                    m.Away.Matches[matchesAtAway-i].ShotsTotal = int.Parse(totShotsStat.Last().ToString());
                                }
                                else
                                {
                                    m.Away.Matches[matchesAtAway-i].ShotsTotal = int.Parse(totShotsStat.First().ToString());
                                }
                                if (m.Away.Matches[matchesAtAway-i].ShotsTotal == 0)
                                {
                                    var shotsOffTarget = match.Stats.Where(s => s.Key.Equals("ShotsOffTarget")).FirstOrDefault();
                                    var totShotsOffTargetStat = shotsOffTarget?.Stats;
                                    if (shotsOffTarget != null && totShotsOffTargetStat.Count == 2)
                                    {
                                        if (latest.Place.Equals("away"))
                                        {
                                            m.Away.Matches[matchesAtAway-i].ShotsTotal = int.Parse(totShotsOffTargetStat.First().ToString());
                                        }
                                        else
                                        {
                                            m.Away.Matches[matchesAtAway-i].ShotsTotal = int.Parse(totShotsOffTargetStat.Last().ToString());
                                        }
                                    }
                                }
                            }
                            var shotsOnTarget = match.Stats.Where(s => s.Key.Equals("ShotsOnTarget")).FirstOrDefault();
                            var shotsOnTargetStat = shotsOnTarget?.Stats;
                            if (shotsOnTargetStat != null && shotsOnTargetStat.Count == 2)
                            {
                                if (latest.Place.Equals("away"))
                                {
                                    m.Away.Matches[matchesAtAway-i].ShotsOnTarget = int.Parse(shotsOnTargetStat.Last().ToString());
                                }
                                else
                                {
                                    m.Away.Matches[matchesAtAway-i].ShotsOnTarget = int.Parse(shotsOnTargetStat.First().ToString());
                                }
                            }
                            var xg = match.Stats.Where(s => s.Key.Equals("expected_goals")).FirstOrDefault();
                            var xgStat = xg?.Stats;
                            if (xgStat != null && xgStat.Count == 2)
                            {
                                if (latest.Place.Equals("away"))
                                {
                                    m.Away.Matches[matchesAtAway - i].ExpectedGoals = decimal.Parse(xgStat.Last().ToString());
                                }
                                else
                                {
                                    m.Away.Matches[matchesAtAway - i].ExpectedGoals = decimal.Parse(xgStat.First().ToString());
                                }
                            }
                        }
                    }
                    else { Console.WriteLine("findPlayedMatch is null"); }
                }
            }
            return m;
        }

        public async Task<string> GetHeader(string findUrl, string findHeader)
        {
            return await _puppeteerService.GetSubRequestHeadersAsync("https://fotmob.com/sv");           
        }

        //public async Task<string> GetHeader(string findUrl, string findHeader)
        //{
        //    try
        //    {
        //        _logger.LogWarning("First in GetHeader");
        //        var browserFetcher = new BrowserFetcher();

        //        // Fetch the latest stable revision manually
        //        var revisionInfo = await browserFetcher.DownloadAsync(BrowserTag.Stable);


        //        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        //        {
        //            Headless = true,
        //            //ExecutablePath = revisionInfo.GetExecutablePath(),  // Correct Chromium path
        //            //ExecutablePath = @"/home/site/wwwroot/tools/chrome/Win64-133.0.6943.98/chrome-win64/chrome.exe",  // Correct Chromium path
        //            //ExecutablePath = @"/home/site/wwwroot/tools/chrome/ungoogled-chromium-133.0.6943.116-1_Win64/chrome.exe",  // Correct Chromium path
        //            ExecutablePath = @"/home/site/wwwroot/tools/chrome/Chrome-bin/chrome.exe",  // Correct Chromium path

        //            Args = new[] { "--no-sandbox" }
        //            //Args = new[]
        //            //{
        //            //    "--no-sandbox",                    // Bypass sandbox security
        //            //    "--disable-setuid-sandbox",        // Disable setuid sandbox
        //            //    "--disable-dev-shm-usage",         // Prevent `/dev/shm` issues
        //            //    "--single-process",                // Run in a single process
        //            //    "--disable-background-networking", // Reduce background tasks
        //            //    "--disable-software-rasterizer",   // Software rasterizer issues
        //            //    "--disable-gpu"                    // No GPU in Azure App Service
        //            //}
        //           // Env = new Dictionary<string, string>
        //           // {
        //           //     ["PATH"] = "/home/site/wwwroot/tools/chrome/Win64-133.0.6943.98/chrome-win64/;" +
        //           //"/home/site/wwwroot/chrome/"
        //           // }
        //             ,DumpIO = true
        //        });

        //        //var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        //        //{
        //        //    Headless = true,
        //        //    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        //        //});

        //        await new BrowserFetcher().DownloadAsync(BrowserTag.Stable);
        //        //var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        //        //var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        //        //{
        //        //    Headless = true,
        //        //    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        //        //});
        //        var page = await browser.NewPageAsync();
        //        string header = string.Empty;
        //        //x-mas: eyJib2R5Ijp7InVybCI6Ii9hcGkvbWF0Y2hlcz9kYXRlPTIwMjUwMjEyIiwiY29kZSI6MTczOTQwMDEzMjIyMywiZm9vIjoicHJvZHVjdGlvbjo2Mjk5ZGRmMTAwMjU3YTg2Y2QzYWY2Y2VmOWNjYWVhMzQ1MTA5YTdhLXVuZGVmaW5lZCJ9LCJzaWduYXR1cmUiOiJCRkNCQ0Y3NkNERDBBQjMwMjVEMzZFMjgxNEM4NTE5QSJ9
        //        page.Request += (sender, e) =>
        //        {
        //            //e.Request.Url
        //            // https://www.fotmob.com/api/matches
        //            if (e.Request.Url.Contains(findUrl))
        //            {
        //                var headers = e.Request.Headers.Where(h => h.Key.Equals(findHeader));
        //                if (headers != null)
        //                {
        //                    header = headers.First().Value;
        //                }
        //            }
        //        };

        //        await page.GoToAsync("https://fotmob.com/sv");
        //        await browser.CloseAsync();

        //        return header;
        //    }
        //    catch(Exception ex) {
        //        _logger.LogError(ex, "GetHeader");
        //        Console.WriteLine(ex.Message);
        //    }
        //    return string.Empty;
        //}
    }
}


/*"
 * 
 * 
 * 
 * fixtures": {
    "allFixtures": {
      "




















": [
        {
          "id": 4373322,
          "pageUrl": "/matches/mjallby-vs-hammarby/27txzw#4373322",
          "opponent": {
            "id": 8127,
            "name": "Mjällby",
            "score": 1
          },
          "home": {
            "id": 8248,
            "name": "Hammarby",
            "score": 1
          },
          "away": {
            "id": 8127,
            "name": "Mjällby",
            "score": 1
          },
          "displayTournament": true,
          "result": 0,
          "notStarted": false,
          "tournament": {
            "name": "Cup",
            "leagueId": 171
          },
          "status": {
            "utcTime": "2024-03-04T17:30:00.000Z",
            "finished": true,
            "started": true,
            "cancelled": false,
            "awarded": false,
            "scoreStr": "1 - 1",
            "reason": {
              "short": "FT",
              "shortKey": "fulltime_short",
              "long": "Full-Time",
              "longKey": "finished"
            }
          }
        },*/