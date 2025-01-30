
using BettingEngine.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace BettingEngine.Services
{

    public interface IAIService
    {
        Task<Prediction> GetDataFromAI(string teams);
    }

    public class AIService : IAIService
    {
        //private const string API_KEY = "FvZsqovqFoAXaF6zepdL5Z0QAZWzzZQ0i4RtOUsGlbVIhkGrj8QqJQQJ99BAACfhMk5XJ3w3AAABACOG6CVl"; // Set your key here
       
        //private const string ENDPOINT = "https://bettingaiservice.openai.azure.com/openai/deployments/gpt-4o/chat/completions?api-version=2024-02-15-preview";

        public HttpClient _httpClient;

        public AIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Prediction> GetDataFromAI(string teams)
        {
            //var requestData = new
            //{
            //    prompt = teams,                
            //    temperature = 0.7,
            //    top_p = 0.95,
            //    max_tokens = 800,
            //    stream = false
            //};

           

                        
            // Define the payload
            var payload = new
            {
                messages = new object[]
                {
            new {
                role = "system",
                content = "Du är en assisten som generera förutsättningar och föreslå ett utfall för en fotbollsmatch baserat på följande faktorer:\r\n" +
                            "1. Nuvarande form\r\n" +
                            "2. Skador\r\n" +
                            "3. Avstängningar\r\n" +
                            "4. Hemma vs Borta\r\n" +
                            "5. Summering\r\n" +
                            "Svaret måste vara i följande JSON format:\r\n" +
                            "{\r\n" +
                            "  \"Probability\": {\r\n" +
                            "    \"One\": \"\",\r\n" +
                            "    \"X\": \"\",\r\n" +
                            "    \"Two\": \"\"\r\n" +
                            "  },\r\n" +
                            "  \"Conditions\": {\r\n" +
                            "    \"CurrentForm\": {},\r\n" +
                            "    \"Injuries\": {},\r\n" +
                            "    \"Suspensions\": {},\r\n" +
                            "    \"HomeVsAway\": {},\r\n" +
                            "    \"Summary\": \"\"\r\n" +
                            "  }\r\n" +
                            "}"
            },
            new {
                role = "user",
                content = "Tillhandahåll data för fotbolls match mellan " + teams + " baserat på förutsättningarna ovan"
            }
                },
                temperature = 0.7,
                top_p = 0.95,
                max_tokens = 800,
                stream = false
            };

            // Serialize the payload to JSON
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(string.Empty, jsonContent);           

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(jsonResponse);

                var content = openAIResponse.Choices[0].Message.Content;

                string cleanedContent = content.Replace("```json", "").Replace("```", "").Trim();

                // Optionally deserialize the cleaned JSON into another object
                var parsedJson = JsonSerializer.Deserialize<object>(cleanedContent);

                var responseData = JsonSerializer.Serialize(parsedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return JsonSerializer.Deserialize<Prediction>(responseData);
                //return await GetMatchData();
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
            }
            
            return null;
        }

        private async Task<Prediction> GetMatchData()
        {
            var matchData = new Prediction
            {
                Probability = new Probability
                {
                    One = "20%",
                    Two = "55%",
                    X = "25%"
                },
                Conditions = new Conditions
                {
                    CurrentForm = new Dictionary<string, string>
        {
            { "Fulham", "Fulham har varit ojämna i sina senaste matcher." },
            { "ManU", "Manchester United visar bättre form." }
        },
                    Injuries = new Dictionary<string, string>
        {
            { "Fulham", "En av deras nyckelspelare är skadad." },
            { "ManU", "Manchester United saknar två viktiga spelare." }
        },
                    Suspensions = new Dictionary<string, string>
        {
            { "Fulham", "Ingen avstängning rapporterad för Fulham." },
            { "ManU", "Casemiro är avstängd." }
        },
                    HomeVsAway = new Dictionary<string, string>
        {
            { "Fulham", "Fulham spelar på hemmaplan." },
            { "ManU", "Manchester United har haft blandade resultat på bortaplan." }
        },
                    Summary = "Manchester United går in som favoriter tack vare bättre form."
                }
            };
            return matchData;
        }
        
    }
}

/*Ta fram förutsättningar med senaste data för fotbollsmatcher baserat på:
Nuvarande form,
Skador,
Avstängningar
HemmaVsBorta
Summering
Sannolikhet för utfallet i matchen.
Svar skall ges i föjande format i JSON:
Data
{
  "Probability": {
    "1": "",
    "2": "",
    "X": ""
  },
  "Conditions": {
    "CurrentForm": {     
    },
    "Injuries": {     
    },
    "Suspensions": {     
    },
    "HomeVsAway": {     
    },
    "Summary": 
  }
}*/