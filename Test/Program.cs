//using System.Net;

//var builder = WebApplication.CreateBuilder(args);

//AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

////builder.WebHost.ConfigureKestrel(serverOptions =>
////{
////    serverOptions.ListenAnyIP(80); // Ensure it's listening on the right port
////});

////builder.WebHost.UseUrls("http://0.0.0.0:80");

//var app = builder.Build();

//app.MapGet("/", () => "Hello from .NET API running in Azure!");

//app.Run();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // Ensure it's not bound to localhost
});

var app = builder.Build();



app.MapGet("/", () => "Hello from ACI!");

app.Run();
