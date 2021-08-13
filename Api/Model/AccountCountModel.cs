using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyBot.Api.Model
{
    public class AccountCountModel
    {
        public AccountCountModel()
        {
            
        }

        public int AccountTypeId { get; set; }
        public string AccountType { get; set; }
        public int AccountCount { get; set; }
    }
}