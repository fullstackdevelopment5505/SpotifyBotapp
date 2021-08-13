using System.Collections.Generic;

namespace SpotifyBot.Api.Model
{
    public class AccountPlayingResponse
    {
        public AccountPlayingResponse()
        {
            States = new List<AccountState>();
        }
        public bool IsPlaying { get; set; }        
        public int orderId { get; set; }
        public List<AccountState> States { get; set; }
    }
}