using System.Threading.Tasks;
using PuppeteerSharp;
using SpotifyBot.Persistence;

namespace SpotifyBot.SpotifyInteraction
{
    public static class SpotifyYourLibraryPage
    {
        
        public static async Task OpenLikedSongsTab(Page page)
        {
            const string selector = "a[href='/collection/tracks']";
            await page.WaitForSelectorAsync(selector);
            await page.ClickAsync(selector);
        }

        public static async Task OpenCurrentTrackPage(Page page, string trackId)
        {
            await page.GoToAsync("https://open.spotify.com/track/" + trackId);

            // Click for enable repeat mode
            //const string selector = ".control-button.spoticon-repeat-16";
            //await page.WaitForSelectorAsync(selector);
            //await page.ClickAsync(selector);
            
        }
    }
}
