using System.Collections.Generic;

namespace SpotifyBot.Api.Model
{
    public class OrderModel
    {
        public OrderModel()
        {
            Tracks = new List<TrackModel>();
        }
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string TrackIds { get; set; }

        public string TrackTitle { get; set; }
        public List<int> AccountCounts { get; set; }

        public int RequiredPlayCount { get; set; }

        public int Priority { get; set; }

        public bool IsActive { get; set; }

        public bool IsProcessed { get; set; }

        public List<TrackModel> Tracks { get; set; }

        public List<JobModel> Jobs { get; set; }

        public string CreatedDate { get; set; }
    }
}