using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SpotifyBot.Api
{
  public class ApiLauncher
  {
    public static Task RunApi(string[] args, Action<IServiceCollection> servicesConfigurator) => new WebHostBuilder()
        .UseStartup<AspNetCoreStartup>()
        .ConfigureServices(servicesConfigurator)
        .UseKestrel()
        .UseUrls("http://*:5005")
        .Build()
        .RunAsync();

  }
}
