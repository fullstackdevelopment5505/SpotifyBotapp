using System;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace SpotifyBot.SpotifyInteraction
{
    public class SpotifyControl
    {
        public static async Task WaitAppearing(Page page)
        {

        }
        
        public static async Task ActivatePlaylistRepeat(Page page)
        {
            const string script = @"(() => {
                let repeatNode = document.querySelector('.spoticon-repeat-16.control-button--active-dot');
                return repeatNode !== null;
            })()";
            bool isRepeatActive = await page.EvaluateExpressionAsync<bool>(script);

            if (!isRepeatActive)
            {
                await page.ClickAsync(".spoticon-repeat-16");
            }
        }

        public static async Task<bool> IsPlaying(Page page)
        {
            return default;
        }

        public static async Task TogglePlayButton(Page page)
        {
            await Task.Delay(1000);
            
            const string script = @"(() => {
                let repeatNode = document.querySelector('button.spoticon-play-16');
                return repeatNode !== null;
            })()";
            bool isPlayButtonExist = await page.EvaluateExpressionAsync<bool>(script);

            if (isPlayButtonExist)
            {
                await page.ClickAsync("button.spoticon-play-16");
            }
        }
        
        public static async Task TogglePauseButton(Page page)
        {
            const string selector = "button.spoticon-pause-16";
            await page.WaitForSelectorAsync(selector);
            await page.ClickAsync(selector);
        }

        public static async Task<TimeSpan> GetPlayedTime(Page page)
        {
            return default;
        }

        public static async Task GoToNextSong(Page page)
        {
            await page.ClickAsync("button.spoticon-skip-forward-16");
        }
        
        public static async Task GoToPlayQueue (Page page)
        {
            await page.ClickAsync(".control-button.spoticon-queue-16");
        }

        public static async Task<string> GetTrackId(Page page)
        {
            await page.WaitForSelectorAsync(".tracklist-col.name");
            await page.ClickAsync(".tracklist-col.name");
            await page.WaitForSelectorAsync(".tracklist-col.more");
            await page.ClickAsync(".tracklist-col.more");
            
            const string script = @"(() => document.querySelector('#main > div > nav.react-contextmenu.react-contextmenu--visible > div:nth-child(5) > textarea').value)()";
            var trackLink = await page.EvaluateExpressionAsync<string>(script);

            return trackLink.Split('/').Last();
        }
    }
}
