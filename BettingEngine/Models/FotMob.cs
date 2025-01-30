using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BettingEngine.Models
{
    //public class General
    //{
    //    [JsonPropertyName("content")]
    //    public Content Content { get; set; }
    //}
    public class Response
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
        [JsonPropertyName("header")]
        public Header Header { get; set; }
    }
    public class Content
    {
        [JsonPropertyName("stats")]
        public FotMobContainerStats Stats { get; set; }
        
    }
    public class Header
    {
        [JsonPropertyName("teams")]
        public List<Team> Teams { get; set; }
    }
    public class FotMobContainerStats
    {
        [JsonPropertyName("Periods")]
        public Periods Periods { get; set; }
    }

    public class Periods
    {
        [JsonPropertyName("All")]
        public AllPeriods All { get; set; }
    }
    public class AllPeriods
    {
        [JsonPropertyName("stats")]
        public List<FotMobMatchStats> Stats { get; set; }

    }
    public class FotMobMatchStats
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }


        [JsonPropertyName("stats")]
        public List<StatItem> Stats { get; set; }

    }

    

    public class StatItem
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("stats")]
        public List<object> Stats { get; set; }
    }



    public class FixtureResponse
    {
        [JsonPropertyName("fixtures")]
        public Fixtures Fixtures { get; set; }
    }

    
    public class Fixtures
    {
        [JsonPropertyName("allFixtures")]
        public AllFixtures AllFixtures { get; set; }
    }

    public class AllFixtures
    {
        [JsonPropertyName("fixtures")]
        public List<Fixture> Fixtures { get; set; }
    }

    public class Fixture
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public string PageUrl { get; set; }
        // public Opponent Opponent { get; set; }
        [JsonPropertyName("home")]

        public Team Home { get; set; }
        [JsonPropertyName("away")]

        public Team Away { get; set; }
        public bool DisplayTournament { get; set; }
        [JsonPropertyName("result")]
        public int Result { get; set; }
        public bool NotStarted { get; set; }
        //public Tournament Tournament { get; set; }
        [JsonPropertyName("status")]
        public Status Status { get; set; }
    }
    public class Status
    {
        [JsonPropertyName("utcTime")]
        public DateTime Date { get; set; }
        [JsonPropertyName("finished")]
        public bool Finished { get; set; }
        public bool Started { get; set; }
        public bool Cancelled { get; set; }
        public bool Awarded { get; set; }
        public string ScoreStr { get; set; }
    }


}
