
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // Ensure it's not bound to localhost
});

var app = builder.Build();

app.Run();
