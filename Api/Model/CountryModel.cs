using System.ComponentModel.DataAnnotations;
namespace SpotifyBot.Api.Model
{
    public class CountryModel
    {
        public int Id { get; set; }

        public string CountryName { get; set; }

        public string CountryCode { get; set; }
    }
}