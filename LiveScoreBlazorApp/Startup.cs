using BettingEngine.Services;
using LiveScoreBlazorApp.Data;
using LiveScoreBlazorApp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;

namespace LiveScoreBlazorApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor()                
            .AddCircuitOptions(options =>
            {
                options.DetailedErrors = true;
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(100);
                options.DisconnectedCircuitMaxRetained = 100;
            });
            services.AddSingleton<WeatherForecastService>();

            services.AddMemoryCache();

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll",
            //        builder =>
            //        {
            //            builder
            //            .AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader()
            //            .AllowCredentials();
            //        });
            //});
            services.AddCors(options =>
            {
                //options.AddPolicy("AllowVueApp", policy =>
                //{
                //    policy.WithOrigins("https://bettingvueapp.azurewebsites.net") // Add the production domain
                //          .AllowAnyHeader()
                //          .AllowAnyMethod()
                //          .AllowCredentials();  // If needed
                //                                //.SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                //});

                options.AddPolicy("AllowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:8080")
                               .AllowAnyHeader()
                               .AllowAnyMethod();                               
                    });
            });

            // Register our own injectables
            services.AddSingleton<IBettingService, BettingService>();            
            services.AddSingleton<ISportRadarService, SportRadarService>();
            services.AddSingleton<IFotMobService, FotMobService>();
            services.AddSingleton<TeamSynonyms>();


            String proxyURL = "http://186.790.3.46:80";
            WebProxy webProxy = new WebProxy(proxyURL);
            services.AddHttpClient<IBettingService, BettingService>(client =>
            {
                client.BaseAddress = new Uri("https://spela.svenskaspel.se/");
                client.DefaultRequestHeaders.Accept
                            .Add(new MediaTypeWithQualityHeaderValue("text/html"));//ACCEPT header

                client.Timeout = TimeSpan.FromMinutes(10);
            });
            //    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            //{
            //    Proxy = webProxy
            //});


            services.AddHttpClient<ISportRadarService, SportRadarService>(client =>
            {
                var mozillaAgent = new ProductInfoHeaderValue(productName: "Mozilla", productVersion: "5.0");
                var chromeAgent = new ProductInfoHeaderValue(productName: "Chrome", productVersion: "127.0.0.0");




                client.BaseAddress = new Uri("https://widgets.fn.sportradar.com/svenskaspel/se/Etc:UTC/gismo/");
                client.DefaultRequestHeaders.Accept
                            .Add(new MediaTypeWithQualityHeaderValue("text/html"));//ACCEPT header
                client.DefaultRequestHeaders.UserAgent.Add(mozillaAgent);
                client.DefaultRequestHeaders.UserAgent.Add(chromeAgent);
                //client.DefaultRequestHeaders.Add("User-Agent", "Other");
                //clientUseDefaultCredentials = true;
                //Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Mobile Safari/537.36

                client.Timeout = TimeSpan.FromMinutes(10);
            });
            //.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            //{
            //    Proxy = webProxy
            //});

            services.AddHttpClient<IFotMobService, FotMobService>(client =>
            {
                client.BaseAddress = new Uri("https://www.fotmob.com/api/");
                client.DefaultRequestHeaders.Add("Accept", "text/html");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                client.Timeout = TimeSpan.FromMinutes(10);
            });

            //services.AddHttpClient<BettingService>(client =>
            //{
            //    client.BaseAddress = new Uri("https://spela.svenskaspel.se/");
            //});

            //services.AddHttpClient<SportRadarService>(client =>
            //{
            //    client.BaseAddress = new Uri("https://widgets.fn.sportradar.com/svenskaspel/se/Etc:UTC/gismo/");
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("AllowSpecificOrigins");
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseCors("AllowVueApp");

            }
           


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            //app.Use(async (context, next) =>
            //{
            //    if (context.Request.Method == "OPTIONS")
            //    {
            //        context.Response.Headers.Add("Access-Control-Allow-Origin", "https://bettingvueapp.azurewebsites.net"); // Replace with your frontend domain
            //        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE");
            //        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            //        context.Response.StatusCode = 204; // No content
            //        return;
            //    }
            //    await next();
            //});

            //app.UseEndpoints(endpoints =>
            //// other settings go here
            //endpoints.MapBlazorHub(options => {
            //    options.WebSockets.CloseTimeout = new TimeSpan(1, 1, 1);
            //    options.LongPolling.PollTimeout = new TimeSpan(1, 0, 0);

            //})
            // );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
