using BettingEngine.Models;

namespace FootballStatApi.Tests
{
    public static class MockFotMobData
    {
        public static List<FotMobMatch> GetMockFotMobMatches()
        {
            return new List<FotMobMatch>
            {
                new FotMobMatch
                {
                    Id = 101,
                    Home = new FotMobTeam { Id = 1, Name = "Manchester United", LongName = "Manchester United FC" },
                    Away = new FotMobTeam { Id = 2, Name = "Chelsea", LongName = "Chelsea FC" }
                },
                new FotMobMatch
                {
                    Id = 102,
                    Home = new FotMobTeam { Id = 3, Name = "Liverpool", LongName = "Liverpool FC" },
                    Away = new FotMobTeam { Id = 4, Name = "Arsenal", LongName = "Arsenal FC" }
                },
                new FotMobMatch
                {
                    Id = 102,
                    Home = new FotMobTeam { Id = 3, Name = "Halmstads BK", LongName = "Halmstads BK" },
                    Away = new FotMobTeam { Id = 4, Name = "Degerfors", LongName = "Degerfors" }
                }
                ,
                new FotMobMatch
                {
                    Id = 102,
                    Home = new FotMobTeam { Id = 3, Name = "IFK Norrköping", LongName = "" },
                    Away = new FotMobTeam { Id = 4, Name = "Östers IF", LongName = "" }
                }
            };
        }
    }
}
