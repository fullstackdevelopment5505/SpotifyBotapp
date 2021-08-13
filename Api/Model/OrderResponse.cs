using System.Collections.Generic;

namespace SpotifyBot.Api.Model
{
    public class OrderResponse
    {
        public string Status { get; set; }
        public List<OrderModel> Orders { get; set; }

        public OrderModel NewOrder { get; set; }
    }

    public class OrderDetailResponse
    {
        public string Status { get; set; }
        public OrderModel Order { get; set; }

    }

    public class JobDetailResponse
    {
        public string Status { get; set; }
        public List<JobModel> Jobs { get; set; }

    }

    public class AccountTrackResponse
    {
        public string Status { get; set; }
        public List<AccountTrackModel> AccountTracks { get; set; }

    }
}