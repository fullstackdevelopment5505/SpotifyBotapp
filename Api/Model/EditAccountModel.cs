using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyBot.Api.Model
{
    public class EditAccountModel
    {
        public int AccountId { get; set; }

        public int ProxyId { get; set; }

        public string AccountEmail { get; set; }

        public int AccountTypeId { get; set; }
        
        public string AccountPwd { get; set; }

        public string AccountCountry { get; set; }

        public string PlayerStatus { get; set; }
    }
}
