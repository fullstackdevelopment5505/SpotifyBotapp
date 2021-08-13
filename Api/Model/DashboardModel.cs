using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyBot.Api.Model
{
    public class DashboardModel
    {
        public DashboardModel() { }

        public int OrderId { get; set; }
        public string OrderTitle { get; set; }
        public string OrderDescription { get; set; }
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string Country { get; set; }
        public string CountryAbbr { get; set; }
        public string AccountType { get; set; }
        public string Songs { get; set; }
        public int CurrentPlays { get; set; }
        public int AllPlays { get; set; }
        public int JobId { get; set; }
        public string ProxyCountry { get; set; }
        public string proxyCountryAbbr { get; set; }
        public string ProxyIpAddress { get; set; }
        public string CreatedDate { get; set; }
        public string Status { get; set; }
        public int AccountCounts { get; set; }
    }
}
