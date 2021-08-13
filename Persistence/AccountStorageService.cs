using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SpotifyBot.Persistence
{
    public sealed class AccountStorageService
    {
        readonly StorageDbContext _db;

        public AccountStorageService(StorageDbContext db) => _db = db;

        public async Task AddAccount(Account account)
        {
            await _db.AddAsync(account);
        }

        public async Task<Account> GetSingleAccount()
        {
            return await _db.Accounts.SingleAsync();
        }
    }
}
