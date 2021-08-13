using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PuppeteerSharp;
using SpotifyBot.Api;
using SpotifyBot.Persistence;

namespace SpotifyBot
{
    static class Program
    {
        public static async Task Main(string[] args)
        {
            var config = await SpotifyAccountsConfig.Read();
            var storageUowProvider = StorageUowProvider.Init();
            // var spotifyServiceGroup = await SpotifyServiceGroup.Create(config);
            var spotifyServiceGroup = await SpotifyServiceGroup.Create(storageUowProvider, config);

            var builder = CreateWebHostBuilder(args, services => services
                .AddSingleton(spotifyServiceGroup)
                .AddSingleton(storageUowProvider));
            
            await spotifyServiceGroup.GetLogStatus(storageUowProvider);
            builder.Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args, Action<IServiceCollection> servicesConfigurator)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
               .ConfigureServices(servicesConfigurator)
               .UseKestrel()
               .UseUrls("http://*:5005")
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseIISIntegration()
               .UseStartup<AspNetCoreStartup>();

            return builder;
        }
    }
}
