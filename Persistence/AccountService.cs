using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SpotifyBot.Api.Model;

namespace SpotifyBot.Persistence
{
    public class AccountService
    {
        readonly StorageDbContext _db;

        public AccountService(StorageDbContext db) => _db = db;

        public async Task AddAccount(AccountModel accountModel)
        {
            var account = new Account()
            {
                AccountId = accountModel.AccountId,
                Email = accountModel.Email,
                Password = accountModel.Password,
                CurrentProxyId = accountModel.CurrentProxyId,
            };

            await _db.Accounts.AddAsync(account);
        }

        public async Task AddAccount(Account accountEntity)
        {
            Account existAccount = _db.Accounts.FirstOrDefault(a => a.Email == accountEntity.Email);
            if(existAccount ==null)
                await _db.Accounts.AddAsync(accountEntity);   
                
        }

        public async Task UpdateAccount(EditAccountModel accountModel)
        {
            //var account = await _db.Accounts.Where(a => a.Email == accountModel.AccountEmail).SingleAsync();
            //if( account != null)
            //{
                var account = await _db.Accounts.Where(a => a.AccountId == accountModel.AccountId).SingleAsync();
                if (account != null)
                {
                    account.Email = accountModel.AccountEmail;
                    account.Password = accountModel.AccountPwd;
                    account.Country = accountModel.AccountCountry;
                    account.AccountTypeId = accountModel.AccountTypeId;
                    account.CurrentProxyId = accountModel.ProxyId;
                    account.PlayerStatus = accountModel.PlayerStatus;

                    _db.Update(account);
                    await _db.SaveChangesAsync();

                }
            //}            
        }

        public async Task UpdateAccountPlaying(int accountId, string status)
        {
            var account = await _db.Accounts.FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (account != null)
            {
                account.PlayerStatus = status;
            }
            _db.SaveChanges();
        }

        public async Task<List<AccountModel>> RemoveAccount(int accountId)
        {
            var account = await _db.Accounts.FirstOrDefaultAsync(x => x.AccountId == accountId);
            var proxyId = account.CurrentProxyId;
            if (account != null)
            {
                _db.Accounts.Remove(account);
                var proxy = await _db.Proxies.FirstOrDefaultAsync(p => p.Id == proxyId);
                if (proxy != null) _db.Proxies.Remove(proxy);
            }
            _db.SaveChangesAsync();
            var accounts = await (from a in _db.Accounts
                                  join at in _db.AccountTypes on a.AccountTypeId equals at.Id
                                  join p in _db.Proxies on a.CurrentProxyId equals p.Id
                                  //join c in _db.Countries on a.Country equals c.CountryName
                                  // where a.AccountId == accountId && a.Status = "Active"
                                  select new AccountModel
                                  {
                                      CurrentProxy = new ProxyModel(p.IpAddress, p.Port, p.UserName, p.Password, p.Country, p.Id),
                                      Email = a.Email,
                                      Password = a.Password,
                                      Country = a.Country,
                                      AccountId = a.AccountId,
                                      AccountType = at.Type,
                                      AccountTypeId = a.AccountTypeId,
                                      CurrentProxyId = a.CurrentProxyId,
                                      PlayerStatus = a.PlayerStatus,
                                      ProxyCountry = p.Country
                                  })
                                  // .Take(2)
                                  .ToListAsync();
            return accounts;

        }

        public async Task<List<AccountModel>> GetAccounts()
        {
            var accounts = await (from a in _db.Accounts
                                  join at in _db.AccountTypes on a.AccountTypeId equals at.Id
                                  join p in _db.Proxies on a.CurrentProxyId equals p.Id
                                  //join c in _db.Countries on a.Country equals c.CountryName
                                  // where a.AccountId == accountId && a.Status = "Active"
                                  select new AccountModel
                                  {
                                      CurrentProxy = new ProxyModel(p.IpAddress, p.Port, p.UserName, p.Password, p.Country,p.Id),
                                      Email = a.Email,
                                      Password = a.Password,
                                      Country = a.Country,
                                      AccountId = a.AccountId,
                                      AccountType = at.Type,
                                      AccountTypeId = a.AccountTypeId,
                                      CurrentProxyId = a.CurrentProxyId,
                                      PlayerStatus = a.PlayerStatus,
                                      ProxyCountry = p.Country
                                  })
                                  // .Take(2)
                                  .ToListAsync();
            return accounts;

        }

        public async Task<AccountModel> GetAccountInfo(int accountId)
        {
            var accountInfo = await (from a in _db.Accounts
                                     join at in _db.AccountTypes on a.AccountTypeId equals at.Id
                                     join p in _db.Proxies on a.CurrentProxyId equals p.Id
                                     where a.AccountId == accountId
                                     select new AccountModel
                                     {
                                         CurrentProxy = new ProxyModel(p.IpAddress, p.Port, p.UserName, p.Password, p.Country, p.Id),
                                         Email = a.Email,
                                         Password = a.Password,
                                         Country = a.Country,
                                         AccountId = a.AccountId,
                                         AccountType = at.Type,
                                         AccountTypeId = a.AccountTypeId,
                                         CurrentProxyId = a.CurrentProxyId,
                                         PlayerStatus = a.PlayerStatus,
                                         ProxyCountry = p.Country
                                     }).SingleAsync();
            return accountInfo;

        }

        public async Task<List<CountryModel>> GetCountrys()
        {
            var countrys = await (from a in _db.Countries
                                  select new CountryModel
                                  {
                                      Id = a.Id,
                                      CountryName = a.CountryName,
                                      CountryCode = a.CountryCode
                                  }).ToListAsync();
            return countrys;
        }
        public async Task<List<AccountModel>> GetProxyAccounts()
        {
            var accounts = await (from a in _db.Accounts
                                  join at in _db.AccountTypes on a.AccountTypeId equals at.Id
                                  join p in _db.Proxies on a.CurrentProxyId equals p.Id
                                  select new AccountModel
                                  {
                                      CurrentProxy = new ProxyModel(p.IpAddress, p.Port, p.UserName, p.Password, p.Country, p.Id),
                                      Email = a.Email,
                                      Password = a.Password,
                                      Country = a.Country,
                                      AccountId = a.AccountId,
                                      AccountType = at.Type,
                                      AccountTypeId = a.AccountTypeId,
                                      CurrentProxyId = a.CurrentProxyId,
                                      PlayerStatus = a.PlayerStatus,
                                      ProxyCountry = p.Country
                                  })
                                  // .Take(2)
                                  .ToListAsync();
            return accounts;

        }

        public string GetUserCountryByIp(string ip)
        {
            IpInfo ipInfo = new IpInfo();
            try
            {
                string info = new WebClient().DownloadString("http://ipinfo.io/" + ip);
                ipInfo = JsonConvert.DeserializeObject<IpInfo>(info);
                RegionInfo myRI1 = new RegionInfo(ipInfo.Country);
                ipInfo.Country = myRI1.EnglishName;
            }
            catch (Exception)
            {
                ipInfo.Country = "";
            }
            //GetCountryAbbreviation(ipInfo.Country);
            return ipInfo.Country;
        }

        public async Task<List<AccountModel>> GetOrderAccounts(int orderId, int accountId)
        {
            var accounts = new List<AccountModel>();
            if( accountId == 0)
            {
                accounts = await (from a in _db.Orders
                                      join at in _db.AccountTracks on a.TrackIds equals at.TrackId
                                      join c in _db.Accounts on at.AccountId equals c.AccountId
                                      join d in _db.AccountTypes on c.AccountTypeId equals d.Id
                                      where a.Id == orderId && at.RequiredPlayCount > at.PlayCount && (c.PlayerStatus == "LoggedIn" || c.PlayerStatus == "Stopped")
                                      select new AccountModel
                                      {
                                          //CurrentProxy = new ProxyModel(p.IpAddress, p.Port, p.UserName, p.Password, p.Id),
                                          Email = c.Email,
                                          Password = c.Password,
                                          Country = c.Country,
                                          AccountId = c.AccountId,
                                          AccountType = d.Type,
                                          AccountTypeId = c.AccountTypeId,
                                          CurrentProxyId = c.CurrentProxyId,
                                          PlayerStatus = c.PlayerStatus
                                      })
                                  .ToListAsync();
            }
            else
            {
                accounts = await (from a in _db.Orders
                                  join at in _db.AccountTracks on a.TrackIds equals at.TrackId
                                  join c in _db.Accounts on at.AccountId equals c.AccountId
                                  join d in _db.AccountTypes on c.AccountTypeId equals d.Id
                                  where a.Id == orderId && c.AccountId == accountId && at.RequiredPlayCount > at.PlayCount && (c.PlayerStatus == "LoggedIn" && c.PlayerStatus == "Playing" && c.PlayerStatus == "Stopped")
                                  select new AccountModel
                                      {
                                      Email = c.Email,
                                      Password = c.Password,
                                      Country = c.Country,
                                      AccountId = c.AccountId,
                                      AccountType = d.Type,
                                      AccountTypeId = c.AccountTypeId,
                                      CurrentProxyId = c.CurrentProxyId,
                                      PlayerStatus = c.PlayerStatus
                                  })
                                  .ToListAsync();
            }
            
            return accounts;

        }


        public async Task<List<AccountTypeModel>> GetAccountTypes()
        {
            return await _db.AccountTypes.Select(a => new AccountTypeModel
            {
                Id = a.Id,
                Type = a.Type,
            }
            ).ToListAsync();
        }

        public async Task<List<AccountCountModel>> GetAccountCounts()//List<AccountCountModel> aCountList
        {
            var aCountList = new List<AccountCountModel>();
            var aTypes = await _db.AccountTypes.ToListAsync();
            foreach (var aType in aTypes)
            {
                var aCount = new AccountCountModel();
                aCount.AccountTypeId = aType.Id;
                aCount.AccountType = aType.Type;
                aCount.AccountCount = 0;
                aCount.AccountCount = _db.Accounts.Where(a => a.AccountTypeId == aType.Id).Count();
                aCountList.Add(aCount);
            }
            return aCountList;
        }

        public async Task AddJob(JobModel jobModel)
        {
            var jobEntity = new Job
            {
                RequiredPlayCount = jobModel.RequiredPlayCount,
                OrderId = jobModel.OrderId
            };

            await _db.Jobs.AddAsync(jobEntity);
        }

        public async Task EditAccount(Account accountEntity)
        {
            _db.Update(accountEntity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> IsAccountByProxyId(int proxyId)
        {
            return await _db.Accounts.AnyAsync(x => x.CurrentProxyId == proxyId);
        }

        public async void SetPlayCount(int orderId, int accountId)
        {
            var order = await _db.Orders.Where(o => o.Id == orderId ).SingleAsync();
            var trackId = order.TrackIds;
            var track = await _db.AccountTracks.Where(at => at.TrackId == trackId && at.AccountId == accountId).SingleAsync();
            track.PlayCount = track.PlayCount + 1;
            _db.Update(track);
            
        }

        public async Task SaveLogStatus(int accountId, string logStatus)
        {
            var account = await _db.Accounts.Where(a => a.AccountId == accountId).SingleAsync();
            account.PlayerStatus = logStatus;
            _db.Update(account);
            await _db.SaveChangesAsync();
        }

        public async Task<AccountListModel> SetAllAccountInfo( AccountListModel accountList)
        {
            List<AccountModel> accounts = new List<AccountModel>() ;
            AccountListModel accountListModel = new AccountListModel();
            foreach ( AccountModel account in accountList.Accounts)
            {
                AccountModel individualAccount = await (from a in _db.Accounts
                                  join at in _db.AccountTypes on a.AccountTypeId equals at.Id
                                  join p in _db.Proxies on a.CurrentProxyId equals p.Id
                                  where a.AccountId == account.AccountId
                                  select new AccountModel
                                  {
                                      CurrentProxy = new ProxyModel(p.IpAddress, p.Port, p.UserName, p.Password, p.Country, p.Id),
                                      Email = a.Email,
                                      Password = a.Password,
                                      Country = a.Country,
                                      AccountId = a.AccountId,
                                      AccountType = at.Type,
                                      AccountTypeId = a.AccountTypeId,
                                      CurrentProxyId = a.CurrentProxyId,
                                      PlayerStatus = a.PlayerStatus,
                                      ProxyCountry = p.Country
                                  }).SingleAsync();
                accounts.Add(individualAccount);
            }
            accountListModel.Accounts = accounts.ToArray();
            return accountListModel;
        }
    }
}