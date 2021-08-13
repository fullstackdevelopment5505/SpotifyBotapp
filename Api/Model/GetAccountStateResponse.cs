using System.Collections.Generic;

namespace SpotifyBot.Api.Model
{
    //todo to be removed as there is a model named AccountPlayingResponse
    public class GetAccountStateResponse
    {
        public GetAccountStateResponse()
        {
            States = new List<AccountState>();
        }

        public bool IsPlaying { get; set; }

        public List<AccountState> States { get; set; }
    }
}