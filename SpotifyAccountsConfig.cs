using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotifyBot
{
    public class SpotifyAccountsConfig
    {
        private const string ConfigFileName = "config.json";

        public AccountInfo[] Accounts { get; set; }

        public static async Task<SpotifyAccountsConfig> Read()
        {
            var json = await File.ReadAllTextAsync(ConfigFileName);

            return JsonConvert.DeserializeObject<SpotifyAccountsConfig>(json);
        }

    }
}