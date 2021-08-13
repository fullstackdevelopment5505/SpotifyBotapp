using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace SpotifyBot.PuppeteerPrelude
{
    public static class PuppeteerExtensions
    {
        public static async Task UseProxyAuth(this Page page, string credentials)
        {
            var authStr = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials))}";
            await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
            {
                ["Proxy-Authorization"] = authStr
            });
        }

        public static async Task TypeAsync(this Frame frame, string selector, string text)
        {
            await (await frame.QuerySelectorAsync(selector)).TypeAsync(text);
        }

        public static async Task ClickAsync(this Frame frame, string selector)
        {
            await (await frame.QuerySelectorAsync(selector)).ClickAsync();
        }

        public static async Task WaitForTruth(this Page page, string script, WaitForFunctionOptions opts = null)
        {
            var jsHandle = await page.WaitForExpressionAsync(script, opts);
            await jsHandle.DisposeAsync();
        }

        public static async Task WaitForDocumentInteractiveState(this Page page, int? timeout = null)
        {
            await page.WaitForTruth("document.readyState === 'interactive' || document.readyState === 'complete'", new WaitForFunctionOptions { Timeout = timeout ?? page.Browser.DefaultWaitForTimeout });
        }
    }
}
