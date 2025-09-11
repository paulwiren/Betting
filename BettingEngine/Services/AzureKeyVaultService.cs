using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingEngine.Services
{
    public interface IAzureKeyVaultService
    {
        Task<string> GetSecretAsync(string key);
    }
    public class AzureKeyVaultService : IAzureKeyVaultService
    {
        private readonly SecretClient _secretClient;
        public AzureKeyVaultService(SecretClient secretClient)
        {
            _secretClient = secretClient;
        }
        public async Task<string> GetSecretAsync(string secretName)
        {
            // DefaultAzureCredential will work locally (with developer login) and in Azure (with Managed Identity)
            //var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value;
        }
    }

    public class NoAzureKeyVaultService : IAzureKeyVaultService
    {
        public async Task<string> GetSecretAsync(string secretName)
        {
            return await Task.FromResult(string.Empty);
        }
    }

}