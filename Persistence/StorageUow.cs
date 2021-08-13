using System;
using System.Threading.Tasks;

namespace SpotifyBot.Persistence
{
    public sealed class StorageUow : IDisposable
    {
        readonly StorageDbContext _ctx;

        public StorageUow(StorageDbContext ctx) => _ctx = ctx;

        public void Dispose() => _ctx.Dispose();

        public async Task ApplyChanges()
        {
            await _ctx.SaveChangesAsync();
        }

        public TrackStorageService TrackStorageService => new TrackStorageService(_ctx);
        
        public AccountStatisticsStorageService AccountStatisticsStorageService => new AccountStatisticsStorageService(_ctx);
        
        public AccountTrackService AccountTrackService => new AccountTrackService(_ctx);

        public AccountService AccountService => new AccountService(_ctx);

        public ProcessService ProcessService => new ProcessService(_ctx);

        public ProxyStorageService ProxyStorageService => new ProxyStorageService(_ctx);
    }
}
