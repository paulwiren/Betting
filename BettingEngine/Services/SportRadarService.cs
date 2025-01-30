using BettingEngine.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Match = BettingEngine.Models.Match;

namespace BettingEngine.Services
{
    public interface ISportRadarService
    {
        Task<BettingBoard> GetCouponAsync(string jsonCoupon, int numberOfMatches = 13);
    }

    public class SportRadarService : ISportRadarService
    {
        public HttpClient _httpClient;
        public IMemoryCache _memoryCache;
        public JsonSerializerOptions _settings;

        //https://api.spela.svenskaspel.se/multifetch?urls=/draw/1/topptipset/draws/2614
        public SportRadarService(HttpClient client, IMemoryCache memoryCache)
        {
            _httpClient = client;
            _memoryCache = memoryCache;
            _settings = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }
        private readonly IEnumerable<Match> Data;

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

            var matches = await GetMatches(ids.Take(numberOfCoupons * numberOfMatches));
            return await CreateBettingBoard(matches, numberOfMatches, dates.ToList());
        }

        private async Task<BettingBoard> CreateBettingBoard(IEnumerable<Match> matches, int numberOfMatches, List<DateTime> dates)
        {
            var games = new List<GameHistory>();
            foreach (var m in matches)
            {
                if (m.Teams == null)
                    continue;
                int homeId = m.Teams.Home.Id;
                int awayId = m.Teams.Away.Id;
                var rows = await GetTeamTablePositions(m);                

                var homeTeamLastMathes = await GetData("stats_team_lastxextended/" + homeId);
                var awayTeamLastMatches = await GetData("stats_team_lastxextended/" + awayId);
                var homeStats = JsonSerializer.Deserialize<Game>(homeTeamLastMathes);
                var awayStats = JsonSerializer.Deserialize<Game>(awayTeamLastMatches);
                var home = new Team { Id = homeId, Name = m.Teams.Home.Name, Matches = homeStats.Doc.First().Data.Matches.Select(x => GetHistoryMatch(homeId, x)).ToList() };
                var away = new Team { Id = awayId, Name = m.Teams.Away.Name, Matches = awayStats.Doc.First().Data.Matches.Select(x => GetHistoryMatch(awayId, x)).ToList() };

                var homePos = rows.Where(x => x.Team.Id == m.Teams.Home.Id).ToList();
                if (homePos != null && homePos.Any())
                {
                    home.TableRow.Pos = homePos.First().Pos;
                    home.TableRow.PosHome = homePos.First().PosHome;
                }

                var awayPos = rows.Where(x => x.Team.Id == m.Teams.Away.Id).ToList();
                if (awayPos != null && awayPos.Any())
                {
                    away.TableRow.Pos = awayPos.First().Pos;
                    away.TableRow.PosAway = awayPos.First().PosAway;
                }

                // Get fotmob data
                games.Add(new GameHistory { Home = home, Away = away });
            }

            var board = new BettingBoard();
            Coupon coupon = new Coupon();
            coupon.GameStop = dates.First();
            int dateIndex = 1;
            for (int i = 0; i<games.Count; i++)
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
        private async Task<IEnumerable<TableRow>> GetTeamTablePositions(Match m)
        {
            var rows = await _memoryCache.GetOrCreateAsync<IEnumerable<TableRow>>(m.LeagueId, async cacheEntry => await GetLeagueData(m));
            return rows;
        }

        private async Task<IEnumerable<TableRow>> GetLeagueData(Match m)
        {
            var leagueData = await GetData("stats_season_tables/" + m.LeagueId);
            if (leagueData == null)
            { 
                return new List<TableRow>();
            }
            try
            {
                var leagueTable = JsonSerializer.Deserialize<Game>(leagueData, _settings);
                var tables = leagueTable.Doc.First().Data.SportRadarTables;
                if (tables != null && tables.Any())
                {
                    return tables.First().TableRows.ToList();
                }
            }
            catch (Exception ex)
            {
                
            }
            return new List<TableRow>();
        }

        private Match GetHistoryMatch(int currentTeamId, Match match)
        {
            var extendedMatch = match;
            extendedMatch.Place = "home";

            if (match.Teams.Away.Id == currentTeamId)
            {
                extendedMatch.Place = "away";
            }            

            // Eval result winner,draw,lost 3,1,0
            var winner = match.Score.Winner;
            match.Score.Winner = "Förlorare";
            if (winner == null)
            {
                match.Score.Winner = "Oavgjort";
                extendedMatch.Score.Points = 1;
            }
            else if (winner.Equals(extendedMatch.Place))
            {
                match.Score.Winner = "Vinnare";
                extendedMatch.Score.Points = 3;
                if (extendedMatch.Place.Equals("away"))
                {
                    extendedMatch.Score.Points += 1;
                }
            }
            return extendedMatch;
        }
        private async Task<IEnumerable<Match>> GetMatches(IEnumerable<string> ids)
        {
            var matches = new List<Match>();
            var settings = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            foreach (var id in ids)
            {
                try
                {
                    var res = await GetData("match_info/" + id  + "/");
                    if (string.IsNullOrEmpty(res)) continue;
                    var game = JsonSerializer.Deserialize<Game>(res, settings);
                    var home = game.Doc.First().Data.Match.Teams.Home;
                    var away = game.Doc.First().Data.Match.Teams.Away;
                    game.Doc.First().Data.Match.LeagueId = game.Doc.First().Data.Tournament.Seasonid;
                    matches.Add(game.Doc.First().Data.Match);
                }
                catch(Exception ex)
                {
                    matches.Add(new BettingEngine.Models.Match());
                }
            }
            return matches;
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

        private async Task<string> GetData(string url)
        {
            // url = "https://widgets.fn.sportradar.com/svenskaspel/se/Etc:UTC/gismo/match_info/41763539/";
            url = "https://www.fotmob.com/sv/matches/chelsea-vs-inter/2eyr1e#4540490:tab=stats";
            try
            {
                // Hämta webbsidan som en sträng
                return await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            return string.Empty;
        }
        
    }

    //private string GetLeagueData()
    //{

    //}
}
