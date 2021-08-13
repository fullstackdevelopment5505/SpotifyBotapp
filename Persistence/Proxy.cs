namespace SpotifyBot.Persistence
{
  public sealed class Proxy
  {
    public int Id { get; set; }

    public string IpAddress { get; set; }

    public ushort Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Country { get; set; }
  }
}
