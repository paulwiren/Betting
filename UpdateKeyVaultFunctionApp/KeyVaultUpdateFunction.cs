using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BettingEngine.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace UpdateKeyVaultFunctionApp
{
    public class KeyVaultUpdateFunction
    {
        private readonly ILogger _logger;
        private readonly PuppeteerService _puppeteerService;
        private readonly SecretClient _secretClient;

        public KeyVaultUpdateFunction(ILoggerFactory loggerFactory, PuppeteerService puppeteerService)
        {
            _logger = loggerFactory.CreateLogger<KeyVaultUpdateFunction>();
            _puppeteerService = puppeteerService;

            _secretClient = new SecretClient(
            new Uri("https://betting-keyvault.vault.azure.net/"),
            new DefaultAzureCredential());
        }

        //https://betting-keyvault.vault.azure.net/secrets/Header-xmas/db4b26a5f7304753bf37bb85ab05e605
        //eyJib2R5Ijp7InVybCI6Ii9hcGkvbWF0Y2hlcz9kYXRlPTIwMjUwNjEzJnRpbWV6b25lPUV1cm9wZSUyRlN0b2NraG9sbSZjY29kZTM9U1dFIiwiY29kZSI6MTc0OTc2NjUzNzUyMCwiZm9vIjoicHJvZHVjdGlvbjo4NjBmYjcxN2Q1OGZjNDkwMjlkOTBiZTE3YmUzMDNlZTkwYzVhNzhiLXVuZGVmaW5lZCJ9LCJzaWduYXR1cmUiOiI5QzRDRjI0OTdBNkMzOTkwOUEwMkNCMkMwNjJEMEYxMCJ9
        [Function("KeyVaultUpdateFunction")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] MyInfo myTimer)
        //public async Task Run([TimerTrigger("0 0 0,12 * * *")] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            var header = await _puppeteerService.GetSubRequestHeadersAsync("https://fotmob.com/sv");


            await _secretClient.SetSecretAsync("Header-xmas", header);
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}