using BettingEngine.Services;
using LiveScoreBlazorApp.Models;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging.AzureAppServices;
using Serilog;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);



//// Add services to the container.
///
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IBettingService, BettingService>();
builder.Services.AddTransient<ICouponService, CouponService>();

builder.Services.AddTransient<IFotMobService, FotMobService>();
builder.Services.AddTransient<IAIService, AIService>();
builder.Services.AddSingleton<TeamSynonyms>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Optional: Configure Azure diagnostics
builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.FileName = "app-logs";
    options.FileSizeLimit = 10 * 1024; // 10MB
    options.RetainedFileCountLimit = 5;
});

//builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);

// builder.Services.GetRequiredService<IConfiguration>()["ApplicationInsights:InstrumentationKey"];
// Load Serilog configuration from appsettings.json
//builder.Host.UseSerilog((context, configuration) =>
//{
//    configuration
//        .ReadFrom.Configuration(context.Configuration)
//        .Enrich.FromLogContext()
//        .WriteTo.Console()
//        .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day)
//        .WriteTo.ApplicationInsights(
//            context.Configuration["ApplicationInsights:InstrumentationKey"],
//TelemetryConverter.Traces);
//});

//builder.Services.AddApplicationInsightsTelemetry(options =>
//{
//    options.ConnectionString = builder.Configuration["ApplicationInsights:InstrumentationKey"];
//});

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/app-log-.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.ApplicationInsights(
            context.Configuration["ApplicationInsights:InstrumentationKey"],
            TelemetryConverter.Traces);
});

TelemetryConfiguration.Active.DisableTelemetry = false;  // Ensure telemetry is enabled

builder.Services.AddHttpClient("LoggedClient")
    .ConfigurePrimaryHttpMessageHandler(() => new LoggingHandler(new HttpClientHandler()));

//APPINSIGHTS_INSTRUMENTATIONKEY

var client = new SecretClient(new Uri("https://betting-keyvault.vault.azure.net/"), new DefaultAzureCredential());
KeyVaultSecret secret = client.GetSecret("Header-xmas");

//var header-xmas = secret.Value;

builder.Services.AddHttpClient<IBettingService, BettingService>(client =>
{
    client.BaseAddress = new Uri("https://spela.svenskaspel.se/");
    client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("text/html"));//ACCEPT header

    client.Timeout = TimeSpan.FromMinutes(10);
});


builder.Services.AddHttpClient<IFotMobService, FotMobService>(client =>
{
    client.BaseAddress = new Uri("https://www.fotmob.com/api/");
    client.DefaultRequestHeaders.Accept
              .Add(new MediaTypeWithQualityHeaderValue("text/html"));//ACCEPT header
    client.DefaultRequestHeaders
                 .Add("X-Fm-Req", "eyJib2R5Ijp7InVybCI6Ii9hcGkvbWF0Y2hOZXdzP2lkPTQxOTY1NjgmY2NvZGUzPVNXRSZsYW5nPXN2IiwiY29kZSI6MTczMjE0MDg5MzA5OCwiZm9vIjoiOTYzYTBjYmE5In0sInNpZ25hdHVyZSI6IkUzODJEQkQ1MzM4NUU4MTQwRjBCNjYxQTE0Qjk4MEU4In0=");
    // client.DefaultRequestHeaders.Add("X-Mas", "sdfsdfs=");
    client.DefaultRequestHeaders.Add("X-Mas", secret.Value);

    client.Timeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddHttpClient<IAIService, AIService>(client =>
{
    client.BaseAddress = new Uri("https://bettingaiservice.openai.azure.com/openai/deployments/gpt-4o/chat/completions?api-version=2024-02-15-preview");
    client.DefaultRequestHeaders.Accept
              .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
    client.DefaultRequestHeaders
              .Add("api-key", "FvZsqovqFoAXaF6zepdL5Z0QAZWzzZQ0i4RtOUsGlbVIhkGrj8QqJQQJ99BAACfhMk5XJ3w3AAABACOG6CVl");
   
    client.Timeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddSingleton<PuppeteerService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//    app.UseRouting();
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers(); // This maps controller routes.
//});

app.MapControllers();



app.Run();
