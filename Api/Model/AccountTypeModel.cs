using System.ComponentModel.DataAnnotations;

namespace SpotifyBot.Api.Model
{
    public class AccountTypeModel
    {
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }
    }
}