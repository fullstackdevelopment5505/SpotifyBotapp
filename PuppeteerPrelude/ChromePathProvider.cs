using System;

namespace SpotifyBot.PuppeteerPrelude
{
    public static class ChromePathProvider
    {
        const string MacOsChromePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
        const string WinChromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
        const string LinuxChromiumPath = "/usr/bin/chromium";

        public static string GetChromePath() =>
            PlatformInfo.IsWindows ? WinChromePath :
            PlatformInfo.IsMac ? MacOsChromePath :
            PlatformInfo.IsLinux ? LinuxChromiumPath
            : throw new ApplicationException("Can not get Chrome path because the OS is not supported.");
    }
}
