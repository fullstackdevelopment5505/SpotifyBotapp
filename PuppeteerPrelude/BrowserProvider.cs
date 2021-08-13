using System;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace SpotifyBot.PuppeteerPrelude
{
    public static class BrowserProvider
    {
        public static async Task<Browser> PrepareBrowser(string proxy,int port, Action<LaunchOptions> launchOptionsConfigurator = null)
        {
            var browserPath = ChromePathProvider.GetChromePath();

            var launchOptions = new LaunchOptions
            {
                ExecutablePath = browserPath,
                Headless = false,
                DefaultViewport = new ViewPortOptions { Width = 0, Height = 0 },
                Args = proxy == null ? new string[0] : new[]
                {
                    /*"--no-sandbox",
                    "--disable-infobars",
                    "--ignore-certificate-errors",
                    "--disable-dev-shm-usage",
                    "--disable-accelerated-2d-canvas",
                    "--disable-gpu",
                    "--window-size=1920x1080",*/
                    $"--proxy-server={proxy}:{port}"
                }
            };
            launchOptionsConfigurator?.Invoke(launchOptions);
            var browser = await Puppeteer.LaunchAsync(launchOptions);
            return browser;
        }

        internal static Task PrepareBrowser(string proxy, LaunchOptions launchOptions)
        {
            throw new NotImplementedException();
        }
    }
}


//$"--proxy-server=\"{proxy}\""
