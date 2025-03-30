using BettingEngine.Models;
using BettingEngine.Services;
using Moq;

namespace FootballStatApi.Tests
{
    public class LevenshteinAlgorithmServiceTests
    {
        private readonly LevenshteinAlgorithmService _levenshteinService;
        private readonly Mock<LevenshteinAlgorithmService> _mockLevenshteinService;

        public LevenshteinAlgorithmServiceTests()
        {
            _levenshteinService = new LevenshteinAlgorithmService();
            _mockLevenshteinService = new Mock<LevenshteinAlgorithmService>();
        }

        [Fact]
        public void FindBestMatch_ShouldReturnCorrectMatch()
        {
            // Arrange

            //string homeTeam = "Halmstad";
            //string awayTeam = "Degerfors";
            string homeTeam = "Norrköpin";
            string awayTeam = "Öster";

            var fotmobMatches = MockFotMobData.GetMockFotMobMatches();
            var footMobLeagues = new List<FotMobLeague> { new FotMobLeague { Matches = fotmobMatches} };

            // Act
            var bestMatch = _levenshteinService.FindBestMatch(homeTeam, awayTeam, footMobLeagues);

            // Assert
            Assert.NotNull(bestMatch);
            Assert.Equal("IFK Norrköping", bestMatch.Home.Name);
            Assert.Equal("Östers IF", bestMatch.Away.Name);
        }

        //[Fact]
        //public void FindBestMatch_ShouldReturnNull_WhenNoGoodMatch()
        //{
        //    // Arrange
        //    var stryktipsetMatch = new Match
        //    {
        //        HomeTeam = "Random Team",
        //        AwayTeam = "Unknown FC",
        //        Date = new DateTime(2025, 3, 30)
        //    };

        //    var fotmobMatches = new List<Match>
        //    {
        //        new Match { HomeTeam = "Manchester United", AwayTeam = "Chelsea FC", Date = new DateTime(2025, 3, 30) },
        //        new Match { HomeTeam = "Liverpool", AwayTeam = "Arsenal", Date = new DateTime(2025, 3, 30) }
        //    };

        //    // Act
        //    var bestMatch = _matchService.FindBestMatch(stryktipsetMatch, fotmobMatches);

        //    // Assert
        //    Assert.Null(bestMatch);
        //}
    }
}