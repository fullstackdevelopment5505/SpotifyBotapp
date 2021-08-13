namespace SpotifyBot.Api.Model
{
    public class AccountState
    {
        public int AccountId { get; set; }

        public bool IsPlaying { get; set; }

        public string PlayerStatus { get; set; }

        public string Email { get; set; }

    }
}