namespace SpotifyBot.Api.Model
{
    public class AccountTrackModel
    {
        public int AccountTrackId { get; set; }

        public int AccountId { get; set; }

        public int JobId { get; set; }

        public string TrackId { get; set; }

        public string Email { get; set; }
        
        public string TrackTitle { get; set; }

        public int RequiredPlayCount { get; set; }

        public int PlayCount { get; set; }
    }
}