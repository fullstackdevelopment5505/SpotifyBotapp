namespace SpotifyBot.Persistence
{
    public sealed class Account
    {
        public int AccountId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Country { get; set; }

        public int CurrentProxyId { get; set; }

        public int AccountTypeId { get; set; }

        public string PlayerStatus { get; set; }

        //public AccountType AccountType { get; set; }
    }
}
