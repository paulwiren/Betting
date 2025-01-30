using BettingEngine.Models;
using BettingEngine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        public IMemoryCache _memoryCache;

        public CouponController(ILogger<CouponController> logger, IBettingService bettingService, IFotMobService fotMobService, IMemoryCache memoryCache, ICouponService couponService, IAIService aiService)
        {
            _logger = logger;
            _bettingService = bettingService;
            _fotMobService = fotMobService;
            _memoryCache = memoryCache;
            _couponService = couponService;
            _aiService = aiService;
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<List<Coupon>>> Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "europatipset";
            // string json = await _bettingService.GetCouponAsync("stryktipset");
            //string json = await _bettingService.GetCouponAsync("topptipset");
            //string json = await _bettingService.GetCouponAsync("europatipset");

            try
            {

                var coupons = new List<Coupon>();
                if (_memoryCache.TryGetValue(name, out coupons))
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
                int c = 0;
                if (matches.Count > 13)
                {
                    nrOfcoupons = matches.Count / 8;
                    take = 8;
                }
                while (nrOfcoupons > 0)
                {

                    var coupon = new Coupon();
                    coupon.GameStop = gameStopDates[c++];
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
                _memoryCache.Set<List<Coupon>>(name, coupons, coupons.First().GameStop.AddHours(2));
                return coupons;
            }
            catch (Exception ex) {
                return NotFound(new { message = "Coupon not found." });
            }
        }

        [HttpGet("game/ai/{couponName}/{gameId}/{teams}")]
        public async Task<ActionResult<List<Coupon>>> GetAIContent(string couponName, int gameId, string teams)
        {

            //var res = await _aiService.GetDataFromAI("Fulham-Man United");
            var coupons = new List<Coupon>();
            if (!_memoryCache.TryGetValue(couponName, out coupons))
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
