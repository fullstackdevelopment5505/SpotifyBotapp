using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotifyBot.Api.Model;

namespace SpotifyBot.Persistence
{
    public sealed class ProxyStorageService
    {
        readonly StorageDbContext _db;
        public ProxyStorageService(StorageDbContext storageDbContext) => _db = storageDbContext;

        public async Task<Proxy> GetProxy(int proxyId)
        {
            return await _db.Proxies.Where(x => x.Id == proxyId).FirstOrDefaultAsync();
        }

        public async Task AddProxy(Proxy proxy)
        {
            await _db.Proxies.AddAsync(proxy);
        }

        public async Task<List<ProxyModel>> GetProxies()
        {
            return await _db.Proxies.Select(a => new ProxyModel(a.IpAddress, a.Port, a.UserName, a.Password,a.Country, a.Id))
                .ToListAsync();
        }

        public async Task RemoveProxy(int proxyId)
        {
            var proxies = await _db.Proxies
                .Where(x => x.Id == proxyId)
                .ToListAsync();

            foreach (var id in proxies)
            {
                _db.Proxies.Remove(id);
            }
        }
        public async Task UpdateProxy(ProxyModel proxyModel)
        {
          var proxy = _db.Proxies.FirstOrDefault(x => x.Id == proxyModel.Id);
          if (proxy != null)
          {
            proxy.IpAddress = proxyModel.IpAddress;
            proxy.Port = proxyModel.Port;
            proxy.UserName = proxyModel.UserName;
            proxy.Password = proxyModel.Password;
            proxy.Country = proxyModel.Country;

            _db.Update(proxy);

            await _db.SaveChangesAsync();

          }
        }
    }
}
