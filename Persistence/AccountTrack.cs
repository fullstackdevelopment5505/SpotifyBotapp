namespace SpotifyBot.Persistence
{
    public class AccountTrack
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string TrackId { get; set; }

        public int JobId { get; set; }

        public int RequiredPlayCount { get; set; }

        public int PlayCount { get; set; }

    }
}