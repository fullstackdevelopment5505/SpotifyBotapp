using SpotifyBot.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyBot.Api.Model
{
    public   class JobModel
    {
        public JobModel(int orderId)
        {
            OrderId = OrderId;
        }
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int TrackId { get; set; }

        public int RequiredPlayCount { get; set; }

        public OrderModel Order { get; set; }

        public TrackModel Track { get; set; }
    }
}
