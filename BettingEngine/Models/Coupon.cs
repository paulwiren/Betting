using System.Text.Json.Serialization;

namespace BettingEngine.Models
{
    public class BettingBoard
    {
        public List<Coupon> Coupons { get; set; } = new List<Coupon>();
    }
    public class Coupon
    {
        public string Id { get; set; }
        public DateTime GameStop { get; set; }
        public List<GameHistory> Games { get; set; } = new List<GameHistory>();

        public List<Percentage> Percentages { get; set; } = new List<Percentage>();
    }

    public class Percentage
    {
        public int Id { get; set; }
        public int Home { get; set; }
        public int Away { get; set; }
        public int Draw { get; set; }
    }
    public class GameHistory
    {
        public int Id { get; set; }
        public Team Home { get; set; }
        public Team Away { get; set; }

        public Prediction Prediction { get; set; }
    }
   
    public class Module
    {
        
        [JsonPropertyName("RegCloseTime")]
        public DateTime CloseDate { get; set; }
       
        public List<DrawEvent> SportsEvents { get; set; }
    }
    public class DrawEvent
    {       
        public string EventDescription { get; set; }
    }


    public class Game
    {
        [JsonPropertyName("doc")]
        public List<Doc> Doc { get; set; }

    }
    public class Doc
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }
    public class Data
    {
        [JsonPropertyName("match")]
        public Match Match { get; set; }

        [JsonPropertyName("matches")]
        public List<Match> Matches { get; set; }

        [JsonPropertyName("tournament")]
        public Tournament Tournament { get; set; }

        [JsonPropertyName("tables")]
        public List<SportRadarTable> SportRadarTables { get; set; }
    }

    public class FotMob
    {
        [JsonPropertyName("leagues")]
        public List<FotMobLeague> Leagues { get; set; }       
    }
    public class FotMobLeague
    {       
        [JsonPropertyName("matches")]
        public List<FotMobMatch> Matches { get; set; }
    }
    public class FotMobMatch
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("home")]
        public FotMobTeam Home { get; set; }
        [JsonPropertyName("away")]
        public FotMobTeam Away { get; set; }
    }
    public class FotMobTeam
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("longName")]
        public string LongName { get; set; }
    }

    public class Match
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("teams")]
        public Teams Teams { get; set; }
        public string Place { get; set; }
        public bool IsHome { get; set; }

        //[JsonPropertyName("time")]
        public DateTime Date { get; set; }

        public int LeagueId { get; set; }

        [JsonPropertyName("result")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Score Score { get; set; } = new Score { Winner = "NA"};

        public int ShotsOnTarget { get; set; }
       // public int ShotsOffTarget { get; set; }
        public int ShotsTotal { get; set; }
        public decimal ExpectedGoals { get; set; }
        public string FotmobUrl { get; set; }
    }
    public class Table
    {
        public List<TableRow> TableRows { get; set; }
      
    }
    public class TableRow
    {
        [JsonPropertyName("team")]
        public Team Team { get; set; }
        [JsonPropertyName("pos")]
        public int Pos { get; set; }
        [JsonPropertyName("posHome")]
        public int PosHome { get; set; }
        [JsonPropertyName("posAway")]
        public int PosAway { get; set; }
    }
    public class Time
    {        
        //public string Date { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }

    public class Teams
    {
        [JsonPropertyName("home")]
        public Team Home { get; set; }
        [JsonPropertyName("away")]
        public Team Away { get; set; }
    }
    public class Score
    {
        [JsonPropertyName("home")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? Home { get; set; }
        [JsonPropertyName("away")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? Away { get; set; }

        [JsonPropertyName("winner")]
        public string Winner { get; set; }

        public int Points { get; set; }

    }
    public class Team
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("mediumname")]
        public string LongName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        public List<Match> Matches { get; set; }
        public TableRow TableRow { get; set; } = new TableRow();

        [JsonPropertyName("score")]
        public int Score { get; set; }

        public Stats Stats { get; set; } = new Stats();
    }
    
    public class Stats
    {
        public int HomeSum { get; set; }
        public int AwaySum { get; set; }
        public int TotSum { get; set; }

    }

    public class Tournament
    {
        [JsonPropertyName("seasonid")]
        public int Seasonid { get; set; }
    }

    public class SportRadarTable
    {
        [JsonPropertyName("tablerows")]
        public List<TableRow> TableRows { get; set; }
    }

   
}
