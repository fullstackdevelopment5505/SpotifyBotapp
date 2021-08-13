using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SpotifyBot.Api
{
    sealed class AspNetCoreStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services) => services
            .With(base.ConfigureServices)
            .AddMvcCore()
            .AddFormatterMappings()
            .AddJsonFormatters()
            .AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                        .AllowAnyMethod()
                                            .AllowAnyHeader()));

        public override void Configure(IApplicationBuilder app) => app.UseMvc().UseCors("AllowAll");
    }
}