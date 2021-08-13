using System.Collections.Generic;

namespace SpotifyBot.Persistence
{
    public sealed class Order
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string TrackIds { get; set; }

        public int RequiredPlayCount { get; set; }

        public int Priority { get; set; }

        public bool IsActive { get; set; }

        public bool IsProcessed { get; set; }

        public List<Job> Jobs { get; set; }

        public string CreatedDate { get; set; }
    }
}
