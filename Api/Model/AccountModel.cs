using System.ComponentModel.DataAnnotations;

namespace SpotifyBot.Api.Model
{
    public class AccountModel
    {
        public int AccountId { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Country { get; set; }

        public int CurrentProxyId { get; set; }

        public ProxyModel CurrentProxy { get; set; }

        public int AccountTypeId { get; set; }

        public string AccountType { get; set; }

        public string PlayerStatus { get; set; }
        public string ProxyCountry { get; set; }
    }
}