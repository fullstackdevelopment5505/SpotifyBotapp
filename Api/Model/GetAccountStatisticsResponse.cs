using System.Collections.Generic;

namespace SpotifyBot.Api.Model
{
    public class GetAccountStatisticsResponse
    {
        public List<TrackStatistic> Statistics { get; set; }
    }
}