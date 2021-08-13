namespace SpotifyBot.Api.Model
{
    public class AccountBriefInfo
    {
        public int AccountId { get; set; }
        public string Email { get; set; }
        
        public AccountState AccountState { get; set; }
    }
}