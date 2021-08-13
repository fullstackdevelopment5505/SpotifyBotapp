using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace SpotifyBot.SpotifyInteraction
{
    public static class SpotifySearchPage
    {
        public static async Task Search(Page page, string text)
        {
            await Task.Delay(2000);
            await page.ClickAsync("a[href='/search']");
            await page.Keyboard.TypeAsync(text);
        }
        
        public static async Task ToggleSongPlaylistStatus(Page page)
        {
            await page.WaitForSelectorAsync(".tracklist-col.name");
            await page.ClickAsync(".tracklist-col.name");
            await page.WaitForSelectorAsync(".tracklist-col.more");
            await page.ClickAsync(".tracklist-col.more");
            await page.WaitForSelectorAsync(".react-contextmenu-item:nth-child(2)");
            await page.ClickAsync(".react-contextmenu-item:nth-child(2)");
        }
    }
}
