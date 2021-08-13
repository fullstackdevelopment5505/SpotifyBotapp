using System.ComponentModel.DataAnnotations;
namespace SpotifyBot.Api.Model
{
    public class ChartModel
    {
        public int rank { get; set; }
        public string trackTitle { get; set; }
        public string trackAuthor { get; set; }
        public string country { get; set; }
        public string stream { get; set; }
        public string daily { get; set; }
    }
}
