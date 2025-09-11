using BettingEngine.Models;
using BettingEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace FootballStatsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ILogger<CouponController> _logger;

        private readonly ICouponService _couponService;
        private readonly IBettingService _bettingService;
        private readonly IFotMobService _fotMobService;
        private readonly IAIService _aiService;
        public ICacheService _cacheService;

        public CouponController(ILogger<CouponController> logger, IBettingService bettingService, IFotMobService fotMobService, ICacheService cacheService, ICouponService couponService, IAIService aiService)
        {
            _logger = logger;
            _bettingService = bettingService;
            _fotMobService = fotMobService;
            _cacheService = cacheService;
            _couponService = couponService;
            _aiService = aiService;
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<List<Coupon>>> Get(string name)
        {
            _logger.LogWarning("GetCoupon()");
            if (string.IsNullOrEmpty(name))
                name = "europatipset";

            return await GetCoupon(name);           
        }

        [HttpGet("RawCoupon/{name}")]
        public async Task<ActionResult<List<Coupon>>> GetRawCoupon(string name)
        {
            _logger.LogInformation($"GetRawCoupon() - {name}");
            if (string.IsNullOrEmpty(name))
                name = "europatipset";

            if (name.Equals("async"))
                name = "topptipset";

            try
            {
                var coupons = new List<Coupon>();
                //if (_cacheService.TryGetValue(name, out coupons))
                //{
                //    await UpdatePrecentages(name, coupons.First());
                //    return coupons;
                //}
                if (_cacheService.TryGetValue(name, out coupons))
                {
                    await UpdatePrecentages(name, coupons?.First());
                    return coupons;
                }
                
                //coupons = new List<Coupon>();

                string json = await _bettingService.GetCouponAsync(name);
                var matches = await _bettingService.GetMatches(json);
                var percentages = await _bettingService.GetPercentage(json);

                var dates = await GetCloseDates(json);
                var gameStopDates = dates.ToList();

                int skip = 0;
                int take = 13;
                int nrOfcoupons = 1;
                int couponIndex = 0;
                int matchNumber = 1;
                if (matches.Count > 13)
                {
                    nrOfcoupons = matches.Count / 8;
                    take = 8;
                }
                while (nrOfcoupons > 0)
                {
                    var coupon = new Coupon();
                    coupon.GameStop = gameStopDates[couponIndex++];
                    foreach (var p in percentages.Skip(skip).Take(take))
                    {
                        coupon.Percentages.Add(p);
                    }
                    foreach (var m in matches.Skip(skip).Take(take))
                    {
                        coupon.Matches.Add(m);
                        coupon.Games.Add(new GameHistory { Id = matchNumber++,  Home = new Team { Name = m.Split('-')[0].Trim() },
                                                           Away = new Team { Name = m.Split('-')[1].Trim() }
                                                        });                        
                    }
                    
                    coupons.Add(coupon);
                    skip += 8;
                    nrOfcoupons--;
                }

                //_memoryCache.Set<DateTime>($"{name}-gameStopDate", dates.First(), dates.First().AddHours(2));
                //_memoryCache.Set<string>($"{name}-json", json, dates.First().AddHours(2));

                var first = dates.First();
                var timespan = first.AddHours(2) - DateTime.Now;
                await _cacheService.SetAsync<DateTime>($"{name}-gameStopDate", dates.First(), timespan);
                await _cacheService.SetAsync<string>($"{name}-json", json, timespan);

                //coupon.GameStop = dates.First();
                //coupon.Percentages = percentages;
                //coupons.Add(coupon);

                return coupons;
            }
            catch (Exception ex)
            {
                return NotFound(new { message = "Coupon not found." });
            }
        }

        //[HttpGet("GetMatch/{name}/{match}")]
        //public async Task<ActionResult<List<Coupon>>> GetMatch(string name, string match)
        //{
        //    try
        //    {
        //        //string json = await _bettingService.GetCouponAsync(name);
        //        //var matches = await _bettingService.GetMatches(json);
        //        //var percentages = await _bettingService.GetPercentage(json);
        //        var coupons = new List<Coupon>();
        //        Coupon coupon = new Coupon();
        //        if (!_memoryCache.TryGetValue(name, out coupons))
        //        {

        //            coupons = new List<Coupon>();                 

        //        }
        //        if (!coupons.Any())
        //        {
        //            var json = _memoryCache.Get<string>($"{name}-json");
        //            var percentages = await _bettingService.GetPercentage(json);
        //            var dates = await GetCloseDates(json);
        //            var gameStopDates = dates.ToList();
        //            coupon = new Coupon();
        //            coupon.GameStop = gameStopDates.First();
        //            coupons.Add(coupon);
        //        }
        //        coupon = coupons.First();
        //        //int index = matches.IndexOf(match);

        //        var stopDate = coupon.GameStop;


        //        // will not work for multiple coupons. stopdate could be later than stopdate
        //        var gh = await _fotMobService.GetStats(match, stopDate);                              
        //        coupon.Games.Add(gh);

        //        _memoryCache.Set<List<Coupon>>(name, coupons, stopDate);
        //        return coupons;
        //    }
        //    catch (Exception ex)
        //    {
        //        return NotFound(new { message = "Match not found." });
        //    }
        //}



        [HttpGet("GetMatch/{name}/{match}/{number}")]
        public async Task<ActionResult<GameHistory>> GetMatch(string name, string match, int number)
        {
            try
            {
                return await GetMatchData(name, match, number);
            }
            catch (Exception ex)
            {
                // Try to get new header value
                //var header = await _fotMobService.GetHeader("https://www.fotmob.com/api/matches", "x-mas");
                //var header = await _fotMobService.GetHeader("https://www.fotmob.com/api/matches", "x-mas");
                //if (string.IsNullOrEmpty(header))
                //{
                //    return await GetMatchData(name, match, number);
                //}
                return NotFound(new { message = "Match not found." });
            }
        }

        [HttpGet("game/ai/{couponName}/{gameId}/{teams}")]
        public async Task<ActionResult<List<Coupon>>> GetAIContent(string couponName, int gameId, string teams)
        {
            if (couponName.Equals("async"))
                couponName = "topptipset";

            //var res = await _aiService.GetDataFromAI("Fulham-Man United");
            var coupons = new List<Coupon>();
            if (!_cacheService.TryGetValue(couponName, out coupons))
            {
                return new List<Coupon>();
            }
            GameHistory selectedgGame = new GameHistory();
            foreach(var c in coupons)
            {
                selectedgGame = c.Games.FirstOrDefault(g => g.Id == gameId);
                if(selectedgGame != null)
                {
                    var prediction = await _aiService.GetDataFromAI(teams);
                    c.Games.FirstOrDefault(g => g.Id == gameId).Prediction = prediction;
                    break;
                }
            }          

            return coupons;
        }

        [HttpGet("game/ai/{gameId}/{teams}")]
        public async Task<ActionResult<Prediction>> GetAIContentAsync(int gameId, string teams)
        {            
            return await _aiService.GetDataFromAI(teams);          
        }

        private async Task<ActionResult<GameHistory>> GetMatchData(string name, string match, int number)
        {
            if (name.Equals("async"))
                name = "topptipset";

            var coupons = new List<Coupon>();
            Coupon coupon = new Coupon();
            if (!_cacheService.TryGetValue(name, out coupons))
            {
                coupons = new List<Coupon>();
            }
            if (!coupons.Any())
            {
                var json = await _cacheService.GetAsync<string>($"{name}-json");
                var percentages = await _bettingService.GetPercentage(json);
                var dates = await GetCloseDates(json);
                var gameStopDates = dates.ToList();
                coupon = new Coupon();
                coupon.GameStop = gameStopDates.First();
                coupons.Add(coupon);
            }
            coupon = coupons.First();
            //int index = matches.IndexOf(match);

            var stopDate = coupon.GameStop;

            // will not work for multiple coupons. stopdate could be later than stopdate
            var gh = await _fotMobService.GetStats(match, stopDate);
            gh = await _fotMobService.GetStats(gh);
            gh.Number = number;
            coupon.Games.Add(gh);

            await _cacheService.SetAsync<List<Coupon>>(name, coupons, stopDate - DateTime.Now);
            return gh;
        }

        private async Task<ActionResult<List<Coupon>>> GetCoupon(string name, int retry = 0)
        {
            try
            {

                var coupons = new List<Coupon>();
                if (_cacheService.TryGetValue(name, out coupons))
                {
                    await UpdatePrecentages(name, coupons.First());
                    return coupons;
                }
                coupons = new List<Coupon>();
                string json = await _bettingService.GetCouponAsync(name);

                var matches = await _bettingService.GetMatches(json);
                var percentages = await _bettingService.GetPercentage(json);
                var dates = await GetCloseDates(json);
                var gameStopDates = dates.ToList();

                int skip = 0;
                int take = 13;
                int nrOfcoupons = 1;
                int couponIndex = 0;
                if (matches.Count > 13)
                {
                    nrOfcoupons = matches.Count / 8;
                    take = 8;
                }
                while (nrOfcoupons > 0)
                {
                    var coupon = new Coupon();
                    coupon.GameStop = gameStopDates[couponIndex++];
                    foreach (var p in percentages.Skip(skip).Take(take))
                    {
                        coupon.Percentages.Add(p);
                    }

                    int id = 1;
                    foreach (var match in matches.Skip(skip).Take(take))
                    {
                        var gh = await _fotMobService.GetStats(match, dates.First());
                        if ((skip == 0 || skip == 8) && gh != null && gh.Id > 0)
                        {
                            gh = await _fotMobService.GetStats(gh);
                        }
                        else
                        {
                            Console.WriteLine($"Couldn't find data on: {match}");
                        }
                        if (gh != null)
                            coupon.Games.Add(gh);
                        //coupon.Games.Add(new GameHistory { Id = id++, Home = new Team { Name = match.Split('-')[0].Trim() }, Away = new Team { Name = match.Split('-')[1].Trim() } });
                    }
                    coupons.Add(coupon);
                    skip += 8;
                    nrOfcoupons--;
                }
                var timeSpan = coupons.First().GameStop.AddHours(2) - DateTime.Now;
                await _cacheService.SetAsync<List<Coupon>>(name, coupons, timeSpan);
                return coupons;
            }
            catch (Exception ex)
            {               
                return NotFound(new { message = ex.Message, Detail = ex.ToString() });
            }
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
        private async Task UpdatePrecentages(string name, Coupon coupon)
        {
            string json = await _bettingService.GetCouponAsync(name);
            var percentages = await _bettingService.GetPercentage(json);
            
            for (int i = 0; i < coupon.Games.Count; i++)
            {
                coupon.Percentages[i] = percentages[i];
                //coupon.Percentages[i] = new Percentage { Away = 10, Draw=10, Home=10 };
            }
        }
    }
}
