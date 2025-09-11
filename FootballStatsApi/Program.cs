using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BettingEngine.Services;
using FootballStatsApi.Toggles;
using LiveScoreBlazorApp.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.AzureAppServices;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
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

// Optional: manually load appsettings.* files
//builder.Configuration
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    //.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
//    .AddJsonFile($"appsettings.Production.json", optional: true, reloadOnChange: true)
//    .AddEnvironmentVariables();


//// Add services to the container.
///
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IBettingService, BettingService>();
builder.Services.AddTransient<ICouponService, CouponService>();

builder.Services.AddTransient<IFotMobService, FotMobService>();
builder.Services.AddTransient<IAIService, AIService>();
builder.Services.AddSingleton<TeamSynonyms>();
builder.Services.AddSingleton<LevenshteinAlgorithmService>();

builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IConnectionMultiplexer>(
//    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));


////ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
///

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IAzureKeyVaultService>(sp =>
{
    var secretClient = new SecretClient(
        new Uri(builder.Configuration["AzureKeyVault"]),
        new DefaultAzureCredential()
    );
    return new AzureKeyVaultService(secretClient);
});
builder.Services.AddSingleton<RedisCacheService>();
builder.Services.AddSingleton<MemoryCacheService>();
builder.Services.AddSingleton<NoCache>();
builder.Services.AddScoped<ICacheService, FeatureToggledCache>();

var redisConnectionString = builder.Configuration["Cache:RedisConnectionString"] ??
    throw new Exception("Redis connection string is missing");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString(redisConnectionString);
});

////Register Redis ConnectionMultiplexer as a singleton
//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//{
//    return ConnectionMultiplexer.Connect(redisConnectionString);
//});



//builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));

// Exponera en (1) IAppCache – vår FeatureToggledCache hanterar allt
builder.Services.AddSingleton<ICacheService, FeatureToggledCache>();

// Localhost 
//var redisOptions = new ConfigurationOptions
//{
//    EndPoints = { builder.Configuration["Redis:Endpoint"] },  // Use Docker's special hostname
//    Password = builder.Configuration["Redis:Password"],
//    Ssl = builder.Environment.IsProduction(),  // Ensure SSL is disabled for local Redis
//    AbortOnConnectFail = false
//};

//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//    ConnectionMultiplexer.Connect(redisOptions)
//);
//-----------------------------------------------------------------

//var redis = ConnectionMultiplexer.Connect(redisConnectionString);
//var db = redis.GetDatabase();
//await db.StringSetAsync("test", "Hello from ACI!");

// Register IDatabase as a scoped service
builder.Services.AddScoped<IDatabase>(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});

// Read Application Insights instrumentation key from environment variables
var appInsightsKey = builder.Configuration["ApplicationInsights:InstrumentationKey"];

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()  // Logs to console (for debugging)
    .WriteTo.ApplicationInsights(
        new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration { ConnectionString = appInsightsKey },
        TelemetryConverter.Traces
    )
    .CreateLogger();

builder.Host.UseSerilog();


//// Set default culture to Swedish
//var swedishCulture = new CultureInfo("sv-SE");
//CultureInfo.DefaultThreadCurrentCulture = swedishCulture;
//CultureInfo.DefaultThreadCurrentUICulture = swedishCulture;

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

    client.DefaultRequestHeaders.Add("X-Mas", "reyJib2R5Ijp7InVybCI6Ii9hcGkvbWF0Y2hlcz9kYXRlPTIwMjUwMzI4JnRpbWV6b25lPUV1cm9wZSUyRlN0b2NraG9sbSZjY29kZTM9U1dFIiwiY29kZSI6MTc0MzExOTkzMzMwNiwiZm9vIjoicHJvZHVjdGlvbjoyNzUxMTkzOGEzZDY0NTA2NzA5ZDU5Yjg5ZTdkMmFmMDZiZDM2OWQxLXVuZGVmaW5lZCJ9LCJzaWduYXR1cmUiOiIzNkNCNkVFOTExOTIzNEU4NzI0NjQ1MDM1ODAzREYzQyJ9");

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

//builder.Services.AddSingleton<PuppeteerService>();



// Only apply this configuration in Production
if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(80); // Ensure it's not bound to localhost
    });
}


var app = builder.Build();

app.UseCors("AllowVueApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();


app.Run();
