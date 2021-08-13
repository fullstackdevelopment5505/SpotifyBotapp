using System.Collections.Generic;

namespace SpotifyBot.Persistence
{
    public sealed class Job
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int TrackId { get; set; }

        public int RequiredPlayCount { get; set; }

        public Order Order { get; set; }

        public Track Track { get; set; }

        public List<AccountTrack> AccountTracks { get; set; }
    }
}
