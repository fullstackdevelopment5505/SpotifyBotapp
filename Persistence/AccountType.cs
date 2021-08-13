using System.Collections.Generic;

namespace SpotifyBot.Persistence
{
    public sealed class AccountType
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public List<Account> Accounts { get; set; }
    }
}
