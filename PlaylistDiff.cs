using SpotifyBot.Persistence;

namespace SpotifyBot
{
    public struct PlaylistDiff
    {
        public Track[] TracksToAdd { get; set; }
        public Track[] TracksToRemove { get; set; }
    }
}