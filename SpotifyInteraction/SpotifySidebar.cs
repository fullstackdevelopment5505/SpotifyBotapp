using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace SpotifyBot.SpotifyInteraction
{
    public static class SpotifySidebar
    {
        public static async Task OpenYourLibrary(Page page)
        {
            const string selector = "a[aria-label='Your Library']";
            await page.WaitForSelectorAsync(selector);
            await page.ClickAsync(selector);
        }

        // TODO: use this 
        public static async Task OpenSearch(Page page)
        {   
            await page.WaitForNavigationAsync();
        }
    }
}
