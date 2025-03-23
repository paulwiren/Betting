using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BettingEngine.Models;
using BettingEngine.Services;
using LiveScoreBlazorApp.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.AzureAppServices;
using Serilog;
using System.Net;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://betting-app.ezfjcdagb9acf8bm.swedencentral.azurecontainer.io")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


//// Add services to the container.
///
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IBettingService, BettingService>();
builder.Services.AddTransient<ICouponService, CouponService>();

builder.Services.AddTransient<IFotMobService, FotMobService>();
builder.Services.AddTransient<IAIService, AIService>();
builder.Services.AddSingleton<TeamSynonyms>();

builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Optional: Configure Azure diagnostics
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
////});

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

//AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;


builder.Services.AddHttpClient("LoggedClient")
    .ConfigurePrimaryHttpMessageHandler(() => new LoggingHandler(new HttpClientHandler()));

//APPINSIGHTS_INSTRUMENTATIONKEY

//var client = new SecretClient(new Uri("https://betting-keyvault.vault.azure.net/"), new DefaultAzureCredential());
//KeyVaultSecret secret = client.GetSecret("Header-xmas");



//var keyVaultName = "betting-keyvault";//Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
//var secretName = "Header-xmas";// Environment.GetEnvironmentVariable("SECRET_NAME");
//var kvUri = $"https://{keyVaultName}.vault.azure.net/";

//var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
//KeyVaultSecret secret = await client.GetSecretAsync(secretName);

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

    client.DefaultRequestHeaders.Add("X-Mas", "eyJib2R5Ijp7InVybCI6Ii9hcGkvbWF0Y2hlcz9kYXRlPTIwMjUwMzE0JnRpbWV6b25lPUV1cm9wZSUyRlN0b2NraG9sbSZjY29kZTM9U1dFIiwiY29kZSI6MTc0MTkxMDMzMTgwNiwiZm9vIjoicHJvZHVjdGlvbjowYjAxMWEwM2U0NGVlZGNkOTI1OWY0NTU2OGVjNjE5NWFiNzU4MTI3LXVuZGVmaW5lZCJ9LCJzaWduYXR1cmUiOiI1ODgxOUQ5OTA4NThGQkE0QkUwMzQxNkYyQzRDQTc3QSJ9");

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

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(5000); // HTTP
//    options.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
//});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // Ensure it's not bound to localhost
});



var app = builder.Build();

app.UseCors("AllowVueApp");

//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Append("Access-Control-Allow-Origin", "http://betting-app.ezfjcdagb9acf8bm.swedencentral.azurecontainer.io");
//    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
//    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

//    if (context.Request.Method == "OPTIONS")
//    {
//        context.Response.StatusCode = 204;
//        await context.Response.CompleteAsync();
//        return;
//    }

//    await next();
//});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers(); // This maps controller routes.
//});

app.MapControllers();



app.MapGet("/", () => "Hello from FootballStatsApi!");

app.Run();
