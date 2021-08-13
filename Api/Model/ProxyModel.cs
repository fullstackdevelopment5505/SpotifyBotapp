namespace SpotifyBot.Api.Model
{
    public sealed class ProxyModel
    {
    public ProxyModel(string ipAddress, ushort port, string username, string password, string country, int id = 0)
    {
      this.IpAddress = ipAddress;
      this.Port = port;
      this.Id = id;
      this.UserName = username;
      this.Password = password;
      this.Country = country;
    }
    public int Id { get; set; }
    public string IpAddress { get; set; }
    public ushort Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Country { get; set; }
  }
}

