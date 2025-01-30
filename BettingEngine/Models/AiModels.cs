using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BettingEngine.Models
{
    public class OpenAIResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public int Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
        public Usage Usage { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("finish_reason")]
        public string Finish_Reason { get; set; }
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }
    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

   public class Prediction
{
    [JsonPropertyName("Probability")]
    public Probability Probability { get; set; }

    [JsonPropertyName("Conditions")]
    public Conditions Conditions { get; set; }
}

public class Probability
{
    [JsonPropertyName("One")]
    public string One { get; set; }

    [JsonPropertyName("Two")]
    public string Two { get; set; }

    [JsonPropertyName("X")]
    public string X { get; set; }
}

public class Conditions
{
        //[JsonPropertyName("CurrentForm")]
        //public CurrentForm CurrentForm { get; set; }

        //[JsonPropertyName("Injuries")]
        //public TeamDetails Injuries { get; set; }

        //[JsonPropertyName("Suspensions")]
        //public TeamDetails Suspensions { get; set; }

        //[JsonPropertyName("HomeVsAway")]
        //public TeamDetails HomeVsAway { get; set; }
        [JsonPropertyName("CurrentForm")]
        public Dictionary<string, string> CurrentForm { get; set; }

        [JsonPropertyName("Injuries")]
        public Dictionary<string, string> Injuries { get; set; }

        [JsonPropertyName("Suspensions")]
        public Dictionary<string, string> Suspensions { get; set; }

        [JsonPropertyName("HomeVsAway")]
        public Dictionary<string, string> HomeVsAway { get; set; }

        [JsonPropertyName("Summary")]
    public string Summary { get; set; }
}

public class CurrentForm
{
    [JsonPropertyName("Fulham")]
    public string Fulham { get; set; }

    [JsonPropertyName("ManU")]
    public string ManU { get; set; }
}

public class TeamDetails
{
    [JsonPropertyName("Fulham")]
    public string Fulham { get; set; }

    [JsonPropertyName("ManU")]
    public string ManU { get; set; }
}

    /*
     * 
     * 
  {"Probability":{"1":"20%","2":"55%","X":"25%"},"Conditions":{"CurrentForm":{"Fulham":"Fulham har varit oj\u00E4mna i sina senaste matcher, med blandade resultat och sv\u00E5rt att hitta stabilitet i anfallet.","ManU":"Manchester United visar b\u00E4ttre form, med flera vinster p\u00E5 sistone, trots vissa problem i defensiven."},"Injuries":{"Fulham":"En av deras nyckelspelare, Andreas Pereira, \u00E4r skadad och f\u00F6rv\u00E4ntas inte spela.","ManU":"Manchester United saknar Lisandro Martinez och Luke Shaw p\u00E5 grund av l\u00E5ngvariga skador, vilket p\u00E5verkar deras f\u00F6rsvar."},"Suspensions":{"Fulham":"Ingen avst\u00E4ngning rapporterad f\u00F6r Fulham.","ManU":"Casemiro \u00E4r avst\u00E4ngd, vilket p\u00E5verkar mittf\u00E4ltets stabilitet."},"HomeVsAway":{"Fulham":"Fulham spelar p\u00E5 hemmaplan, vilket kan ge dem en viss f\u00F6rdel, men de har inte varit starka hemma denna s\u00E4song.","ManU":"Manchester United har haft blandade resultat p\u00E5 bortaplan men lyckats vinna viktiga matcher."},"Summary":"Manchester United g\u00E5r in som favoriter tack vare b\u00E4ttre form och en starkare trupp, men deras defensiva skador och Casemiros avst\u00E4ngning kan ge Fulham chansen att \u00F6verraska p\u00E5 hemmaplan."}}
     */
}
