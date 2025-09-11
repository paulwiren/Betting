using BettingEngine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UpdateKeyVaultFunctionApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication().
                ConfigureServices(services =>
                {
                    services.AddSingleton<PuppeteerService>();
                })
                .Build();

            host.Run();
        }
    }
}