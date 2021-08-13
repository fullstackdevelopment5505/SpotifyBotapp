using System;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace SpotifyBot.SpotifyInteraction
{
    public static class SpotifyLoginPage
    {
        public static async Task<string> OpenMainPage(Page page)
        {
            String logStatus = "Added";
            try {
                var response = await page.GoToAsync("https://open.spotify.com", timeout: 0);
                if(response.Ok == false && response.StatusText.Equals("Proxy Authentication Required"))
                {
                    logStatus = "ProxyUserInfoFail";
                }
            }
            catch (Exception)
            {
                logStatus = "ProxyAddressFail";
            }
            return logStatus;
            //await page.setDefaultNavigationTimeout(0);
        }

        public static async Task ClickSignIn(Page page)
        {
            var x = await page.QuerySelectorAsync("div#main > div > div.Root__top-container > div.Root__main-view > div.main-view-container > div.main-view-container__scroll-node > div.main-view-container__scroll-node-child > header > div > div:nth-child(3) > button:nth-child(2) ");
            if (x == null)
            {
                await page.WaitForSelectorAsync("div#main > div > div.Root__top-container > div.Root__main-view > div.main-view-container > div.main-view-container__scroll-node > div.main-view-container__scroll-node-child > header > div > div:nth-child(4) > button:nth-child(2) ");
                await page.ClickAsync("div#main > div > div.Root__top-container > div.Root__main-view > div.main-view-container > div.main-view-container__scroll-node > div.main-view-container__scroll-node-child > header > div > div:nth-child(4) > button:nth-child(2) ");
            }
            else
            {
                await page.WaitForSelectorAsync("div#main > div > div.Root__top-container > div.Root__main-view > div.main-view-container > div.main-view-container__scroll-node > div.main-view-container__scroll-node-child > header > div > div:nth-child(3) > button:nth-child(2) ");
                await page.ClickAsync("div#main > div > div.Root__top-container > div.Root__main-view > div.main-view-container > div.main-view-container__scroll-node > div.main-view-container__scroll-node-child > header > div > div:nth-child(3) > button:nth-child(2) ");
            }            
        }

        public static async Task<bool> SignIn(Page page, string login, string password)
        {
            var initialUrl = page.Url;
            await page.WaitForSelectorAsync("#login-username");
            await page.ClickAsync("#login-username");
            await page.Keyboard.TypeAsync(login);
            await page.ClickAsync("#login-password");
            await page.Keyboard.TypeAsync(password);
            await page.WaitForSelectorAsync("#login-button");
            await page.ClickAsync("#login-button");
            return initialUrl != page.Url;
        }
    }
}
