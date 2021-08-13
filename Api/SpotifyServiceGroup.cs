using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotifyBot.Api.Model;
using SpotifyBot.Persistence;
using SpotifyBot.PuppeteerPrelude;

namespace SpotifyBot.Api
{
    public class SpotifyServiceGroup
    {
        private Dictionary<int, SpotifyService> _spotifyServices;
        private SpotifyAccountsConfig _config;

        private SpotifyServiceGroup(Dictionary<int, SpotifyService> spotifyServices, SpotifyAccountsConfig config)
        {
            _config = config;
            _spotifyServices = spotifyServices;

        }

        /// <summary>
        /// Creates spotify service group from credentials present at the config
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static async Task<SpotifyServiceGroup> Create(SpotifyAccountsConfig config)
        {
            var spotifyServices = new Dictionary<int, SpotifyService>();
            foreach (var accountInfo in config.Accounts)
            {
                spotifyServices.Add(accountInfo.AccountId, await SpotifyService.Create(accountInfo));
            }

          // Create a master service
            spotifyServices.Add(0, await SpotifyService.CreateMaster());

            return new SpotifyServiceGroup(spotifyServices, config);
        }

        /// <summary>
        /// Creates spotify service instances from credentials received from database
        /// </summary>
        /// <returns></returns>
        public static async Task<SpotifyServiceGroup> Create(StorageUowProvider storageUowProvider, SpotifyAccountsConfig config)
        {
            var spotifyServices = new Dictionary<int, SpotifyService>();
            List<AccountModel> allAccounts;

            using (var uow = storageUowProvider.CreateUow())
            {
                allAccounts = await uow.AccountService.GetProxyAccounts();// get all account with proxy
            }

            foreach (var accountModel in allAccounts)
            {
                var accountInfo = GetAccountInfoFromAccountModel(accountModel);
                spotifyServices.Add(accountInfo.AccountId, await SpotifyService.Create( accountInfo));//open web browser and login by users
            }

          // Create a master service with the credentials from the config file
            spotifyServices.Add(0, await SpotifyService.CreateMaster());//create empty spotifyservice
            spotifyServices.Add(30000, await SpotifyService.CreateChart(30000));
            return new SpotifyServiceGroup(spotifyServices, config);
        }

        public static async Task<SpotifyServiceGroup> Create(StorageUowProvider storageUowProvider, SpotifyAccountsConfig config, AccountModel[] allAccounts,SpotifyServiceGroup spotifyServiceGroup)
        {
            var spotifyServices = spotifyServiceGroup._spotifyServices;
            foreach (var accountModel in allAccounts)
            {
                var accountInfo = new AccountInfo();
                accountInfo.SpotifyCredentials.Login = accountModel.Email;
                accountInfo.SpotifyCredentials.Password = accountModel.Password;
                accountInfo.AccountId = accountModel.AccountId;
                accountInfo.Proxy = new ProxyData
                {
                    IpAddress = accountModel.CurrentProxy.IpAddress,
                    Port = accountModel.CurrentProxy.Port,
                    UserName = accountModel.CurrentProxy.UserName,
                    Password = accountModel.CurrentProxy.Password
                };
                spotifyServices.Add(accountInfo.AccountId, await SpotifyService.Create(accountInfo));//open web browser and login by users
            }

            // Create a master service with the credentials from the config file
            //spotifyServices.Add(0, await SpotifyService.CreateMaster());//create empty spotifyservice
            return new SpotifyServiceGroup(spotifyServices, config);
        }

        public SpotifyService GetService(int accountId)
        {
            return _spotifyServices[accountId];
        }

        public SpotifyService GetMainService()
        {
            return _spotifyServices[0];
        }

        public AccountBriefInfo[] GetBriefInfo()
        {
            var briefInfo = new List<AccountBriefInfo>();
            foreach (var accountInfo in _config.Accounts)
            {
                var accountId = accountInfo.AccountId;
                briefInfo.Add(new AccountBriefInfo()
                {
                    Email = accountInfo.SpotifyCredentials.Login,
                    AccountId = accountId,
                    AccountState = new AccountState() { IsPlaying = GetService(accountId).IsPlaying() }
                });
            }

            return briefInfo.ToArray();
        }

        public async Task<Persistence.Track[]> GetPlaylist(StorageUowProvider storageUowProvider)
        {
            Persistence.Track[] allTracks;

            using (var uow = storageUowProvider.CreateUow())
            {
                allTracks = await uow.TrackStorageService.GetAllTracks();
                await uow.ApplyChanges();
            }

            return allTracks;
        }

        private static AccountInfo GetAccountInfoFromAccountModel(AccountModel accountModel)
        {
            var accountInfo = new AccountInfo();
            accountInfo.SpotifyCredentials.Login = accountModel.Email;
            accountInfo.SpotifyCredentials.Password = accountModel.Password;
            accountInfo.AccountId = accountModel.AccountId;
            accountInfo.Proxy = new ProxyData
            {
                IpAddress = accountModel.CurrentProxy.IpAddress,
                Port = accountModel.CurrentProxy.Port,
                UserName = accountModel.CurrentProxy.UserName,
                Password = accountModel.CurrentProxy.Password
            };

            return accountInfo;
        }

        public async Task<bool> IsAccountLinkedWithProxyId(StorageUowProvider storageUowProvider, int proxyId)
        {
            var result = false;

            using (var uow = storageUowProvider.CreateUow())
            {
                result = await uow.AccountService.IsAccountByProxyId(proxyId);
            }

            return result;
        }

        public async Task GetLogStatus(StorageUowProvider storageUowProvider)
        {
            List<AccountModel> allAccounts;
            using (var uow = storageUowProvider.CreateUow())
            {
                allAccounts = await uow.AccountService.GetProxyAccounts();
            }

            foreach (var accountModel in allAccounts)
            {
                if (!accountModel.PlayerStatus.Contains("Proxy"))
                {
                    var spotifyService = _spotifyServices[accountModel.AccountId];
                    await spotifyService.GetLogStatus(storageUowProvider, accountModel.AccountId);
                }                
            }
        }

        public async Task GetAddAccountLogStatus(StorageUowProvider storageUowProvider,AccountModel[] accountList)
        {
            foreach(var accountModel in accountList)
            {
                var spotifyService = _spotifyServices[accountModel.AccountId];
                await spotifyService.GetLogStatus(storageUowProvider, accountModel.AccountId);
            }
            
        }

    }
}