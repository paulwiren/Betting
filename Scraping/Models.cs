using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Scraping
{

    public class GameHistory
    {
        public Team Home { get; set; }
        public Team Away { get; set; }
    }

    public class DrawEvent
    {
        public string EventDescription { get; set; }
    }
    public class Cupong
    {
        public string Engine { get; set; }
        public List<DrawEvent> Draws { get; set; }
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
    }

    public class Match
    {
        [JsonPropertyName("teams")]
        public Teams Teams { get; set; }

        [JsonPropertyName("result")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Score? Score { get; set; }
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
        [JsonPropertyName("uid")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }

        public List<Match> Matches { get; set; }

        public Stats Stats { get; set; } = new Stats();
    }
    public class Stats
    {
        public int Sum { get; set; }
    }
}
