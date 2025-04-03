using BettingEngine.Models;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace BettingEngine.Services
{
    public class LevenshteinAlgorithmService
    {
        private readonly ILogger<LevenshteinAlgorithmService> _logger;

        public LevenshteinAlgorithmService(ILogger<LevenshteinAlgorithmService> logger)
        {
            _logger = logger;
        }

        public FotMobMatch FindBestMatch(string homeTeam, string awayTeam, IEnumerable<FotMobLeague> leagues)
        {
            _logger.LogInformation($"FindBestMatch: {homeTeam}-{awayTeam}");
            int highestScore = 0;
            foreach (var league in leagues)
            {
                foreach (var fotmobMatch in league.Matches)
                {
                    int homeScore = LevenshteinDistance(homeTeam, fotmobMatch.Home.Name);
                    int awayScore = LevenshteinDistance(awayTeam, fotmobMatch.Away.Name);
                    int dateDiff = 0; // Math.Abs((stryktipsetMatch.Date - fotmobMatch.Date).Days);

                    int matchScore = 100 - (homeScore + awayScore) / 2;

                    if (matchScore > highestScore && matchScore > 95) // 70 is the threshold
                    {
                        _logger.LogInformation($"FindBestMatch: Score {homeTeam}-{awayTeam} = {matchScore}");
                        return fotmobMatch;
                    }
                }
            }
            _logger.LogInformation($"FindBestMatch: No match found {homeTeam}-{awayTeam}");
            return null;
        }

        public int LevenshteinDistance(string s1, string s2)
        {
            if (s1 == null) s1 = "";
            if (s2 == null) s2 = "";

            int len1 = s1.Length;
            int len2 = s2.Length;
            var dp = new int[len1 + 1, len2 + 1];

            for (int i = 0; i <= len1; i++)
                for (int j = 0; j <= len2; j++)
                    if (i == 0) dp[i, j] = j;
                    else if (j == 0) dp[i, j] = i;
                    else dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + (s1[i - 1] == s2[j - 1] ? 0 : 1)
                    );

            return dp[len1, len2];
        }
    }
}
