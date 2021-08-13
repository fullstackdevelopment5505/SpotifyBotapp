using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SpotifyBot.Persistence
{
    public sealed class AccountStatisticsStorageService
    {
        readonly StorageDbContext _db;

        public AccountStatisticsStorageService(StorageDbContext db) => _db = db;

        async Task<AccountTrackPlayStatistics> GetOrCreateStatistics(int accountId, string trackId)
        {
            var dbSet = _db.ProfileTrackPlayStatistics;
            var stat = await dbSet.FirstOrDefaultAsync(
                x => x.AccountId == accountId && x.TrackId == trackId
            );
            if (stat != null) return stat;

            stat = new AccountTrackPlayStatistics { AccountId = accountId, TrackId = trackId };
            await dbSet.AddAsync(stat);
            return stat;
        }

        async Task<AccountTrack> UpdateStatistics(int accountId, string trackId)
        {
            var dbSet = _db.AccountTracks;
            var stat = await dbSet.FirstOrDefaultAsync(
                x => x.AccountId == accountId && x.TrackId == trackId
            );

            return stat;
        }

        public async Task HandleTrackPlayedEvent(int accountId, string trackId)
        {
            // var stats = await GetOrCreateStatistics(accountId, trackId);
            // We are now updating the play count at th Account Track Table itself
            var stats = await UpdateStatistics(accountId, trackId);
            if (stats != null)
            {
                stats.PlayCount++;
            }
        }

        public async Task<int> GetTrackPlaysCount(int accountId, string trackId)
        {
            var stats = await GetOrCreateStatistics(accountId, trackId);
            return stats.CountOfPlays;
        }

        public async Task RemoveAccountPlayStatistics(int accountId)
        {
            var accountTrackPlays = await _db.ProfileTrackPlayStatistics
                .Where(x => x.AccountId == accountId)
                .ToListAsync();

            foreach (var accountTrackPlay in accountTrackPlays)
            {
                _db.ProfileTrackPlayStatistics.Remove(accountTrackPlay);
            }
        }
    }
}
