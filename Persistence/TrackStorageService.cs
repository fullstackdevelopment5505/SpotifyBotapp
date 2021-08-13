using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SpotifyBot.Persistence
{
    public sealed class TrackStorageService
    {
        readonly StorageDbContext _db;
        public TrackStorageService(StorageDbContext db) => _db = db;


        public async Task<Track> GetTrackById(string id)
        {
            return await _db.Tracks.FirstOrDefaultAsync(x => x.TrackId == id);
        }

        public async Task<Track> GetTrackByTitle(string title)
        {
            return await _db.Tracks.FirstOrDefaultAsync(x => x.Title == title);
        }

        public async Task<Track> AddTrack(string id, string title)
        {
            var track = new Track { TrackId = id, Title = title };
            await _db.Tracks.AddAsync(track);
            return track;
        }

        public async Task<Track[]> GetAllTracks()
        {
            return await _db.Tracks.ToArrayAsync();
        }

    }
}
