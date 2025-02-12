using System.Drawing;
using System.Threading.Tasks;

namespace LiveScoreBlazorApp.Models
{
    public class TeamSynonyms
    {
        private Dictionary<string, string> Names { get; set; } = new Dictionary<string, string>();

        public async Task<IEnumerable<string>> FindTeamNameSynonyms(string teamName, string longName)
        {
            var synonyms = new List<string>();
            var synonym = Names.Where(t => t.Key.ToLower() == teamName.Trim().ToLower()).ToList();
            if (synonym != null && synonym.Any())
            {
                var name = synonym.First().Value;
                synonyms = Names.Where(t => t.Value.ToLower() == name.ToLower()).Select(t => t.Key).ToList();
            }
            if (!string.IsNullOrEmpty(longName))
                synonyms.Insert(0, longName.Trim());
            if (!string.IsNullOrEmpty(teamName))
                synonyms.Insert(0, teamName.Trim());

            return synonyms.Distinct();
        }
        public TeamSynonyms() {

            //Sweden
            Names.Add("Malmö FF", "Malmö FF");
            Names.Add("Landskon", "Landskrona BoIS");
            Names.Add("Landskona", "Landskrona BoIS");
            Names.Add("Landskona Bois", "Landskrona BoIS");
            Names.Add("GIF Sundsvall", "GIF Sundsvall");
            Names.Add("GIF Sunds", "GIF Sundsvall");
            Names.Add("Värnamo", "IFK Värnamo");
            Names.Add("IFK Värnamo", "IFK Värnamo");
            Names.Add("Malmö", "Malmö FF");
            Names.Add("FC København", "Köpenhamn");
            Names.Add("Köpenhamn", "Köpenhamn");
            Names.Add("Djurgårde", "Djurgården");
            Names.Add("Djurgården", "Djurgården");

            //England
            Names.Add("Manchester U", "Manchester U");
            Names.Add("Manchester United", "Manchester U");
            Names.Add("Man United", "Manchester U");
            Names.Add("Man U", "Manchester U");
            Names.Add("Man Utd", "Manchester U");
            Names.Add("Manchester C", "Manchester C");
            Names.Add("Manchester City", "Manchester C");
            Names.Add("Man C", "Manchester C");
            Names.Add("Man City", "Manchester C");
            Names.Add("Nottingham", "Nottingham Forest");
            Names.Add("Nottm Forest", "Nottingham Forest");
            Names.Add("Nottingha", "Nottingham Forest");
            Names.Add("Nottingham Forest", "Nottingham Forest");
            Names.Add("Wolverhampton", "Wolverhampton");
            Names.Add("Southampton", "Southampton");
            Names.Add("Southampt", "Southampton");
            Names.Add("Wolves", "Wolverhampton");
            Names.Add("Aston Villa", "Aston Villa");
            Names.Add("A Villa", "Aston Villa");
            Names.Add("Aston V", "Aston Villa");
            Names.Add("Bournemou", "Bournemouth");
            Names.Add("Bournemouth", "Bournemouth");
            Names.Add("Sheffield U", "Sheffield U");
            Names.Add("Sheffield United", "Sheffield U");
            Names.Add("Sheff United", "Sheffield U");
            Names.Add("Sheff U", "Sheffield U");
            Names.Add("Sheff Utd", "Sheffield U");
            Names.Add("Sheffield W", "Sheffield W");
            Names.Add("Sheffield Wednesday", "Sheffield W");
            Names.Add("Sheffield Wed", "Sheffield W");
            Names.Add("Sheff Wednesday", "Sheffield W");
            Names.Add("Sheff W", "Sheffield W");
            Names.Add("Sheff Wed", "Sheffield W");
            Names.Add("Queens Park Rangers", "Queens Park Rangers");
            Names.Add("QPR", "Queens Park Rangers");
            Names.Add("W.B.A.", "West Bromwich");
            Names.Add("West Bromwich", "West Bromwich");
            Names.Add("West Brom", "West Bromwich");
            Names.Add("Bristol R", "Bristol Rovers");
            Names.Add("Bristol Rovers", "Bristol Rovers");
            Names.Add("Bristol C", "Bristol City");
            Names.Add("Bristol City", "Bristol City");
            Names.Add("Oxford", "Oxford Utd");
            Names.Add("Oxford Utd", "Oxford Utd");
            Names.Add("Oxford U", "Oxford U");
            Names.Add("Oxford United", "Oxford United");
            Names.Add("Portsmout", "Portsmouth");
            Names.Add("Portsmouth", "Portsmouth");
            Names.Add("Middlesbr", "Middlesbrough");
            Names.Add("Middlesbrough", "Middlesbrough");
            Names.Add("Crystal P", "Crystal Palace");
            Names.Add("Crystal Palace", "Crystal Palace");
            Names.Add("Sunderland", "Sunderland");
            Names.Add("Sunderlan", "Sunderland");
            Names.Add("Northampt", "Northampton");
            Names.Add("Northampton", "Northampton");
            Names.Add("Plymouth", "Plymouth");
            Names.Add("Plymouth Argyle", "Plymouth");
            Names.Add("Birmingha", "Birmingham");
            Names.Add("Birmingham", "Birmingham");



            /// Spain
            Names.Add("Real Madrid", "Real Madrid");
            Names.Add("R.Madrid", "Real Madrid");
            Names.Add("R Madrid", "Real Madrid");
            Names.Add("At.Madrid", "Atlético Madrid");
            Names.Add("At. Madrid", "Atlético Madrid");
            Names.Add("Atlético Madrid", "Atlético Madrid");
            Names.Add("Atletico Madrid", "Atlético Madrid");
            Names.Add("A Madrid", "Atlético Madrid");
            Names.Add("Celta de Vigo", "Celta de Vigo");
            Names.Add("Barcelona", "Barcelona");
            Names.Add("Barca", "Barcelona");
            Names.Add("FC Barcelona", "Barcelona");
            Names.Add("R.Sociedad", "Real Sociedad");
            Names.Add("R. Sociedad", "Real Sociedad");
            Names.Add("Real Sociedad", "Real Sociedad");
            Names.Add("Sociedad", "Real Sociedad");
            Names.Add("At.Bilbao", "Atlético Bilbao");
            Names.Add("At. Bilbao", "Atlético Bilbao");
            Names.Add("Atletico Bilbao", "Atlético Bilbao");
            Names.Add("At Bilbao", "Athletic Club");
            Names.Add("Athletic Club", "Athletic Club");
            Names.Add("Atlétic Bilbao", "Athletic Club");
            Names.Add("Villarrea", "Villarreal");
            Names.Add("Montpelli", "Montpellier");
            Names.Add("Montpellier", "Montpellier");
            Names.Add("Vallecano", "Rayo Vallecano");
            Names.Add("Rayo Vallecano", "Rayo Vallecano");           
            Names.Add("Villarreal", "Villarreal");

            //Italy
            Names.Add("Milan", "Milan");
            Names.Add("AC Milan", "Milan");
            Names.Add("Fiorentin", "Fiorentina");
            Names.Add("Fiorentina", "Fiorentina");

            //Belgium
            Names.Add("Alkmaar", "Alkmaar");
            Names.Add("AZ Alkmaar", "Alkmaar");
            Names.Add("Royale Union Saint-Gilloise", "Royale Union SG");
            Names.Add("Royale Union Saint - Gilloise", "Royale Union SG");
            Names.Add("Royale Union SG", "Royale Union SG");            
            Names.Add("Union St.Gilloise", "Royale Union SG");
            Names.Add("Club Brugge", "Club Brügge");
            Names.Add("Brugge", "Club Brügge");
            Names.Add("Club Brüg", "Club Brügge");
            Names.Add("Brügge", "Club Brügge");
            Names.Add("Club Brügge", "Club Brügge");
            Names.Add("Sporting Charleroi", "Charleroi");
            Names.Add("Charleroi", "Charleroi");           
            Names.Add("Standard Liege", "Standard Liege");
            Names.Add("Standard", "Standard Liege");



            //France
            Names.Add("RC Strasbourg", "Strasbourg");
            Names.Add("Strasbourg", "Strasbourg");

            //Scottish
            Names.Add("Rangers", "Glasgow Rangers");
            Names.Add("Glasgow Rangers", "Glasgow Rangers");
            Names.Add("Forfar Athletic", "Forfar");
            Names.Add("Forfar", "Forfar");
            Names.Add("St Johnstone", "St Johnstone");
            Names.Add("St. Johnstone", "St Johnstone");
            Names.Add("St.Johnstone", "St Johnstone");
            Names.Add("Heart of Midlothian", "Hearts");
            Names.Add("Hearts", "Hearts");
            Names.Add("St. Mirren", "St Mirren");
            Names.Add("St.Mirren", "St Mirren");
            Names.Add("St Mirren", "St Mirren");
            Names.Add("Spartans FC", "Spartans FC");
            Names.Add("Spartans", "Spartans FC");
    
            //Germany
            Names.Add("Saarbrucken", "Saarbrucken");
            Names.Add("Saarbrücken", "Saarbrucken");
            Names.Add("Mönchengladbach", "Mönchengladbach");
            Names.Add("Borussia Mönchengladbach", "Mönchengladbach");
            Names.Add("M'gladbach", "Mönchengladbach");
            Names.Add("Köln", "FC Köln");
            Names.Add("FC Köln", "FC Köln");
            Names.Add("Werder Bremen", "Werder Bremen");
            Names.Add("Bremen", "Werder Bremen");
            Names.Add("FC Heidenheim", "FC Heidenheim");
            Names.Add("Heidenheim", "FC Heidenheim");
            Names.Add("Heidenhei", "FC Heidenheim");
            Names.Add("RB Leipzig", "RB Leipzig");
            Names.Add("Leipzig", "RB Leipzig");
            Names.Add("VfB Stuttgart", "VfB Stuttgart");
            Names.Add("Stuttgart", "VfB Stuttgart");
            Names.Add("Bayer Lev", "Leverkusen");
            Names.Add("Leverkusen", "Leverkusen");
            Names.Add("RB Leipzi", "RB Leipzig");
            Names.Add("Bayern Mü", "Bayern München");
            Names.Add("Bayern München", "Bayern München");
            Names.Add("VfL Wolfsburg", "VfL Wolfsburg");
            Names.Add("Wolfsburg", "VfL Wolfsburg");


            Names.Add("SSV Ulm", "Ulm");
            Names.Add("Ulm", "Ulm");

            //Schweiz            
            Names.Add("FC Zuerich", "Zürich");
            Names.Add("Zürich", "Zürich");
            Names.Add("Zurich", "Zürich");
            Names.Add("Servette", "Servette");
            Names.Add("Servette Geneva", "Servette");
            Names.Add("Young Boy", "Young Boys");
            Names.Add("Young Boys", "Young Boys");


            Names.Add("Ludogorets", "Ludogorets Razgrad");
            Names.Add("Ludogorets Razgrad", "Ludogorets Razgrad");
            
            //Portugal            
            Names.Add("Sporting CP", "Sporting CP");
            Names.Add("Sporting", "Sporting CP");
            Names.Add("Sp Lissab", "Sporting CP");
            Names.Add("Porto", "FC Porto");
            Names.Add("FC Porto", "FC Porto");


            // Netherlands
            Names.Add("PSV Eindh", "PSV Eindhoven");
            Names.Add("PSV Eindhoven", "PSV Eindhoven");

            //Czech Republic
            Names.Add("Sparta Prague", "Sparta Prague");
            Names.Add("Sparta", "Sparta Prague");
            Names.Add("Sparta Prag", "Sparta Prague");
            Names.Add("Slavia Prague", "Slavia Prague");
            Names.Add("Sl. Prag", "Slavia Prague");
           

            //Ukraine
            Names.Add("Sjakhtar Donetsk", "Shakhtar Donetsk");
            Names.Add("Sjakhtar", "Sjakhtar Donetsk");
            Names.Add("Shakhtar Donetsk", "Shakhtar Donetsk");
            Names.Add("Shakhtar", "Shakhtar Donetsk");


            //Norway
            Names.Add("Bodø/Glimt", "Bodø/Glimt");
            Names.Add("Bodø/Glim", "Bodø/Glimt");
            Names.Add("Bodö/Glim", "Bodø/Glimt");
            Names.Add("Bodö/Glimt", "Bodö/Glimt");


            Names.Add("D. Zagreb", "Dinamo Zagreb");
            Names.Add("Dinamo Zagreb", "Dinamo Zagreb");

            //Denmark
            Names.Add("FC Midtjylland", "FC Midtjylland");
            Names.Add("Midtjylla", "FC Midtjylland");

            //
            Names.Add("Vitória", "Vitoria de Guimaraes");
            Names.Add("Vitoria de Guimaraes", "Vitoria de Guimaraes");
            Names.Add("St. Gallen", "St. Gallen");
            Names.Add("St. Galle", "St. Gallen");
            Names.Add("Panathina", "Panathinaikos");
            Names.Add("Panathinaikos", "Panathinaikos");
            Names.Add("New Saint", "TNS");
            Names.Add("TNS", "TNS");
            Names.Add("Rapid Wie", "Rapid Wien");
            Names.Add("Rapid Wien", "Rapid Wien");
            Names.Add("Omonia Nicosia", "Omonia Nicosia");
            Names.Add("Omonia Ni", "Omonia Nicosia");
            Names.Add("Shamrock", "Shamrock Rovers");
            Names.Add("Shamrock Rovers", "Shamrock Rovers");
            Names.Add("FK Borac", "Borac Banja Luka");
            Names.Add("Borac Banja Luka", "Borac Banja Luka");
            Names.Add("Jagiellon", "Jagiellonia Bialystok");
            Names.Add("Jagiellonia Bialystok", "Jagiellonia Bialystok");
            Names.Add("FK Mlada", "Mlada Boleslav");
            Names.Add("Mlada Boleslav", "Mlada Boleslav");
            Names.Add("Besiktas", "Besiktas");
            Names.Add("Legia War", "Legia Warszawa");
            Names.Add("Legia Warszawa", "Legia Warszawa");
            Names.Add("Vikingur", "Vikingur Reykjavik");
            Names.Add("Vikingur Reykjavik", "Vikingur Reykjavik");
            Names.Add("LASK Linz", "LASK");
            Names.Add("LASK", "LASK");
            


            //International
            Names.Add("Skottland", "Scotland");
            Names.Add("Kroatien", "Croatia");
            Names.Add("Polen", "Poland");
            Names.Add("Scotland", "Skottland");
            Names.Add("Croatia", "Kroatien");
            Names.Add("Poland", "Polen");

        }

    }
}
