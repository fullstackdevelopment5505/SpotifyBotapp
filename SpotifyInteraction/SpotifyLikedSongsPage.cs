using System.Threading.Tasks;
using PuppeteerSharp;

namespace SpotifyBot.SpotifyInteraction
{
    public static class SpotifyLikedSongsPage
    {
        public static async Task ClickOnFirstSong(Page page)
        {
            const string selector = ".position.tracklist-top-align";
            await page.WaitForSelectorAsync(selector);
            await page.ClickAsync(selector);
        }
    }
}
