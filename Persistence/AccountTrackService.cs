using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotifyBot.Api.Model;
using System;

namespace SpotifyBot.Persistence
{
    public class AccountTrackService
    {
        readonly StorageDbContext _db;
        public AccountTrackService(StorageDbContext db) => _db = db;

        public async Task AddTrack( string trackId, string trackTitle)
        {
            var trackEntity = _db.Tracks.FirstOrDefault(x => x.TrackId == trackId);

            if (trackEntity == null)
            {
                var track = new Track()
                {
                    Title = trackTitle,
                    TrackId = trackId
                };
                await _db.Tracks.AddAsync(track);
            }
        }
        
        public async Task RemoveTrack(int accountId, int orderId)
        {
            
            var order =await _db.Orders.Where(a => a.Id == orderId).Select(o => o).SingleAsync();
            var trackId = "";
            var isProcessed = false;
            if( order != null)
            {
                trackId = order.TrackIds;
                isProcessed = order.IsProcessed;
                _db.Orders.Remove(order);
            }
            var job = await _db.Jobs.Where(b => b.OrderId == orderId).Select(j => j).SingleAsync();
            var jobId = 0;
            if( job != null)
            {
                jobId = job.Id;
                _db.Jobs.Remove(job);
            }

            if (isProcessed)
            {
                var accountTracks = await _db.AccountTracks.Where(c => (c.JobId == jobId && c.TrackId == trackId)).Select(at => at).ToListAsync();
                if (accountTracks != null)
                {
                    foreach(var atrack in accountTracks)
                        _db.AccountTracks.Remove(atrack);
                }
            }
          
        }

        public async Task EditTrack(int accountId, int orderId, NewTrackDataModel newTrackData)
        {
            var oldtrackId = "";
            var jobId = 0;
            var requiredPlayCount = newTrackData.playCount;
            var order =await _db.Orders.Where(a => a.Id == orderId).Select(o => o).SingleAsync();
            if( order != null)
            {
                oldtrackId = order.TrackIds;
                var track = await _db.Tracks.Where(t => t.TrackId == order.TrackIds).SingleAsync();
                track.TrackId = newTrackData.trackId;
                track.Title = newTrackData.trackTitle;
                _db.Update(track);

                order.TrackIds = newTrackData.trackId;
                order.RequiredPlayCount = newTrackData.playCount;
                _db.Update(order);

                var job = await _db.Jobs.Where(j => j.OrderId == order.Id && j.TrackId == track.Id).SingleAsync();
                if( job != null)
                {
                    jobId = job.Id;
                    job.RequiredPlayCount = newTrackData.playCount;
                    _db.Update(job);
                }

                var accounts = await _db.Accounts.Where(a => a.PlayerStatus != "Credentail" && a.PlayerStatus != "ProxyAddressFail").ToListAsync();
                var totalAccounts = accounts.Count();
                double actualCount = (double)requiredPlayCount / totalAccounts;

                var accountTracks = await _db.AccountTracks.Where(at => at.JobId == jobId && at.TrackId == oldtrackId).ToListAsync();
                if(accountTracks != null)
                {
                    foreach(var accountTrack in accountTracks) {
                        accountTrack.TrackId = newTrackData.trackId;
                        accountTrack.RequiredPlayCount = GetRoundedValue(actualCount);
                        _db.Update(accountTrack);
                    }
                    
                }
                await _db.SaveChangesAsync();
            }            
           
        }

        private int GetRoundedValue(double input)
        {
            return Convert.ToInt32(Math.Ceiling(input));
        }

        public async Task<Track[]> GetAccountTracks(int accountId)
        {
            return await _db.Tracks
                .ToArrayAsync();
        }

        public async Task<AccountTrackModel[]> GetAccountTracks()
        {
            return await _db.AccountTracks
                .Join(_db.Tracks,
                    a => a.TrackId,
                    t => t.TrackId,
                    (a, t) => new AccountTrackModel()
                    {
                        TrackId = t.TrackId,
                        PlayCount = a.PlayCount
                    })
                .ToArrayAsync();
        }

        public async Task<AccountTrackModel[]> GetNextAccountTrackModels(int accountId, int currentAccountTrackId = 0)
        {
            return await _db.AccountTracks
                .Where(x => x.AccountId == accountId && (x.RequiredPlayCount - x.PlayCount) > 0 && x.Id > currentAccountTrackId)
                .OrderBy(at => at.Id )
                .Select(at => new AccountTrackModel()
                {
                    AccountTrackId = at.Id,
                    AccountId = at.AccountId,
                    TrackId = at.TrackId,
                    TrackTitle = ""
                })
                .ToArrayAsync();
        }

        public async Task<int> GetAccountTracksCount(int accountId)
        {
            return await _db.AccountTracks
                .Where(x => x.AccountId == accountId && (x.RequiredPlayCount - x.PlayCount) > 0)
                .CountAsync(); 
        }
    }
}