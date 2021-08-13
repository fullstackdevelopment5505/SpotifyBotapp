using SpotifyBot.Persistence;

namespace SpotifyBot.Api.Model
{
    public class TrackModel
    {
        public TrackModel(Track tr)
        {
            Id = tr.Id;
            TrackId = tr.TrackId;
            TrackTitle = tr.Title;
        }

        public TrackModel() { }
        public int Id { get; set; }

        public string TrackId { get; set; }

        public string TrackTitle { get; set; }

    }
}