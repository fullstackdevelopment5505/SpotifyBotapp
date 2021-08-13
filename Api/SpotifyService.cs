using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;
using SpotifyBot.Api.Model;
using SpotifyBot.Persistence;
using SpotifyBot.PuppeteerPrelude;
using SpotifyBot.Shared;
using SpotifyBot.SpotifyInteraction;

namespace SpotifyBot.Api
{
    public class SpotifyService
    {
        private readonly Page _page;
        private CancellationTokenSource _cancelTokenSource;
        private bool _isPlaylistPlaying;
        private int _accountId;

        private SpotifyService(Page page, int accountId)
        {
            _accountId = accountId;
            _page = page;
        }

        private SpotifyService()
        {
        }

        public static async Task<SpotifyService> Create(AccountInfo config)
        {
          // todo proxy not working , will remove it after getting proper proxy
          // config.Proxy.IpAddress = null;
            var browser = await BrowserProvider.PrepareBrowser(proxy: config.Proxy.IpAddress, port: config.Proxy.Port);     
            var pages = await browser.PagesAsync();
            var page = pages.Single();
            string logstatus = "";
            Credentials proxyCredentails = new Credentials
            {
                Username = config.Proxy.UserName,
                Password = config.Proxy.Password
            };
            await page.AuthenticateAsync(proxyCredentails);
            logstatus = await SingIn(page, config.SpotifyCredentials);
            var storageUowProvider = StorageUowProvider.Init();
            using(var uow = storageUowProvider.CreateUow())
            {
                await uow.AccountService.SaveLogStatus(config.AccountId, logstatus);
            }
            return new SpotifyService(page, config.AccountId);
        }
        public static async Task<SpotifyService> CreateChart(int num)
        {
            var browserPath = ChromePathProvider.GetChromePath();

            var launchOptions = new LaunchOptions
            {
                ExecutablePath = browserPath,
                Headless = false,
                DefaultViewport = new ViewPortOptions { Width = 0, Height = 0 },
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-infobars",
                    "--ignore-certificate-errors",
                    "--disable-dev-shm-usage",
                    "--disable-accelerated-2d-canvas",
                    "--disable-gpu",
                    "--window-size=1920x1080",
                }
            };
            ///launchOptionsConfigurator?.Invoke(launchOptions);
            var browser = await Puppeteer.LaunchAsync(launchOptions);
            // Create a new page and go to Bing Maps
            Page page = await browser.NewPageAsync();
            await page.GoToAsync("https://spotifycharts.com/regional");
            
            return new SpotifyService(page, num);
        }

        public static async Task<SpotifyService> CreateMaster()
        {
          return new SpotifyService();
        }

        private static async Task<string> SingIn(Page page, SpotifyCredentials spotifyCredentials)
        {
          string logStatus = await SpotifyLoginPage.OpenMainPage(page);
          if (logStatus.Equals("Added")) {
            await SpotifyLoginPage.ClickSignIn(page);
            bool loginStatus = await SpotifyLoginPage.SignIn(page, spotifyCredentials.Login, spotifyCredentials.Password);
            logStatus = "LoggedIn";
            if (!loginStatus)
            {
                logStatus = "";
            }        
          }
          return logStatus;
          //SaveLoginStatus(int accountId, string logStatus);
        } 

        private PlaylistDiff GetDiff(Persistence.Track[] newPlaylist, Persistence.Track[] currentPlaylist)
        {
          Console.WriteLine(4);
          var playlistDiff = new PlaylistDiff()
          {
            TracksToAdd = newPlaylist.Where(x => currentPlaylist.All(y => y.TrackId != x.TrackId)).ToArray(),
            TracksToRemove = currentPlaylist.Where(x => newPlaylist.All(y => y.TrackId != x.TrackId)).ToArray()
          };

          var tracks = newPlaylist.Where(x => currentPlaylist.All(y => y.TrackId != x.TrackId)).ToArray();
          foreach (var track in tracks)
          {
            Console.WriteLine(track.Title);
          }

          return playlistDiff;
        }

        public async Task StopPlaylist()
        {
          _isPlaylistPlaying = false;

          if (_cancelTokenSource != null)
          {
            _cancelTokenSource.Cancel();
            await SpotifyControl.TogglePauseButton(_page);
          }

          _cancelTokenSource = null;
        }

        //public async Task StopPlayyingAll(StorageUowProvider storageUowProvider)
        //{
        //    var allAccounts = new List<AccountModel>();

        //    using (var uow = storageUowProvider.CreateUow())
        //    {
        //        allAccounts = await uow.AccountService.GetAccounts();
        //    }

        //    return allAccounts.ToArray();
        //}

        private async Task HandleTrackPlayedEvent(StorageUowProvider storageUowProvider)
        {
          var trackId = await SpotifyControl.GetTrackId(_page);

          using (var uow = storageUowProvider.CreateUow())
          {

            await uow.AccountStatisticsStorageService.HandleTrackPlayedEvent(_accountId, trackId);
            await uow.ApplyChanges();
          }
        }

        /// <summary>
        /// This method plays the tracks sequencially from user's liked songs list
        /// TODO: Deprecated , not used any more
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <returns></returns>
        public async Task StartPlaylist(StorageUowProvider storageUowProvider)
        {
          _isPlaylistPlaying = true;
          _cancelTokenSource = new CancellationTokenSource();
          CancellationToken token = _cancelTokenSource.Token;
          await SpotifySidebar.OpenYourLibrary(_page);
          await SpotifyYourLibraryPage.OpenLikedSongsTab(_page);
          await SpotifyLikedSongsPage.ClickOnFirstSong(_page);
          await SpotifyControl.ActivatePlaylistRepeat(_page);
          await SpotifyControl.GoToPlayQueue(_page);

          while (!token.IsCancellationRequested)
          {
            await Task.Delay(33000 + new Random().Next(0, 10000), token);
            await HandleTrackPlayedEvent(storageUowProvider);
            await SpotifyControl.GoToNextSong(_page);
          }
        }

        /// <summary>
        /// This method plays the tracks as provided by the user in his account tracks list
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="currentAccountTrackId"></param>
        /// <returns></returns>
        public async Task StartParticularTracks(StorageUowProvider storageUowProvider, int currentAccountTrackId = 0)
        {
          _isPlaylistPlaying = true;
          _cancelTokenSource = new CancellationTokenSource();
          CancellationToken token = _cancelTokenSource.Token;
          var accountTracks = await GetAccountTrackModels(storageUowProvider, currentAccountTrackId);
          if (accountTracks.Count() > 0)
          {
            currentAccountTrackId = accountTracks.First().AccountTrackId;
            var currentTrackId = accountTracks.First().TrackId;
            await SpotifyYourLibraryPage.OpenCurrentTrackPage(_page, currentTrackId);
            while (!token.IsCancellationRequested)
            {
              await Task.Delay((AppConstants.Configurations.MinimumTrackPlayingDurationInSeconds * 1000) + new Random().Next(0, 10000), token);
              await HandleTrackPlayedEvent(storageUowProvider);
              await StartParticularTracks(storageUowProvider, currentAccountTrackId);
            }
          }
          else
          {
            // if you have reached here then it has two cases either there are no tracks for th e user 
            // or we have reached the end of the list
            var userTracksCount = await GetAccountTracksCount(storageUowProvider);
            if (userTracksCount > 0)
            {
              await StartParticularTracks(storageUowProvider, 0);
            }
            else
            {
              using (var uow = storageUowProvider.CreateUow())
              {
                UpdateAccountPlayingStatus(storageUowProvider, "Complete");
                await StopPlaylist();
              }
            }
          }
        }

        public async void SetPlayCount(StorageUowProvider storageUowProvider, int orderId, int accountId)
        {
            using (var uow = storageUowProvider.CreateUow())
            {
                    uow.AccountService.SetPlayCount(orderId, accountId);
                    await uow.ApplyChanges();
            }
        }
        public async void UpdateAccountPlayingStatus(StorageUowProvider storageUowProvider, string status)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            await uow.AccountService.UpdateAccountPlaying(_accountId, status);
            await uow.ApplyChanges();
          }
        }

        public bool IsPlaying()
        {
          var accountstate = new AccountState() { IsPlaying = _isPlaylistPlaying };
          return accountstate.IsPlaying;
        }

        public async Task AddTrack(
            StorageUowProvider storageUowProvider,
            int accountId, string trackId, string trackTitle)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            await uow.AccountTrackService.AddTrack(trackId, trackTitle);
            await uow.ApplyChanges();
          }
        }

        public async Task RemoveTrack(StorageUowProvider storageUowProvider, int orderId)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            await uow.AccountTrackService.RemoveTrack(_accountId, orderId);
            await uow.ApplyChanges();
          }
        }

        public async Task EditTrack(StorageUowProvider storageUowProvider, int orderId, NewTrackDataModel newTrackData)
        {
            using (var uow = storageUowProvider.CreateUow())
            {
                await uow.AccountTrackService.EditTrack(_accountId, orderId, newTrackData);
                await uow.ApplyChanges();
            }
        }

            public async Task<List<TrackStatistic>> GetStatistics(StorageUowProvider storageUowProvider)
        {
          var trackStatistics = new Dictionary<string, int>(); // will refine later

          using (var uow = storageUowProvider.CreateUow())
          {
            var allTracks = await uow.AccountTrackService.GetAccountTracks();
            foreach (var track in allTracks)
            {
              if (trackStatistics.ContainsKey(track.TrackId))
              {
                trackStatistics[track.TrackId] = trackStatistics[track.TrackId] + track.PlayCount;
              }
              else
              {
                trackStatistics.Add(track.TrackId, track.PlayCount);
              }
            }
          }

          return GetTrackStatistics(trackStatistics);
        }

        private List<TrackStatistic> GetTrackStatistics(Dictionary<string, int> stats)
        {
          List<TrackStatistic> retResult = new List<TrackStatistic>();
          foreach (KeyValuePair<string, int> stat in stats)
          {
            retResult.Add(new TrackStatistic
            {
              TrackId = stat.Key,
              PlaysCount = stat.Value
            });
          }
          return retResult;
        }

        public async Task<AccountModel[]> GetAccounts(StorageUowProvider storageUowProvider)
        {
          var allAccounts = new List<AccountModel>();

          using (var uow = storageUowProvider.CreateUow())
          {
            allAccounts = await uow.AccountService.GetAccounts();
          }

          return allAccounts.ToArray();
        }

        public async Task<AccountModel> GetAccountInfo(StorageUowProvider storageUowProvider, int accountId)
        {
            var account = new AccountModel();

            using (var uow = storageUowProvider.CreateUow())
            {
                account = await uow.AccountService.GetAccountInfo(accountId);
            }

            return account;
        }

        public async Task<AccountModel[]> GetOrderAccounts(StorageUowProvider storageUowProvider, int orderId, int accountId)
        {
                var allAccounts = new List<AccountModel>();

                using (var uow = storageUowProvider.CreateUow())
                {
                    allAccounts = await uow.AccountService.GetOrderAccounts(orderId, accountId);
                }

                return allAccounts.ToArray();
        }

        public async Task<List<OrderModel>> GetOrders(StorageUowProvider storageUowProvider, int orderId = 0)
        {
          var allOrders = new List<OrderModel>();

          using (var uow = storageUowProvider.CreateUow())
          {
            allOrders = await uow.ProcessService.GetOrders( orderId);
          }

          return allOrders;
        }

        public async Task<OrderModel> GetOrderDetail(StorageUowProvider storageUowProvider, int orderId)
        {
          var orderDetail = new OrderModel();

          using (var uow = storageUowProvider.CreateUow())
          {
            orderDetail = await uow.ProcessService.GetOrderDetail(orderId);
          }

          return orderDetail;
        }

        public async Task<List<JobModel>> GetOrderJobDetail(StorageUowProvider storageUowProvider, int orderId)
        {
          var jobs = new List<JobModel>();
          using (var uow = storageUowProvider.CreateUow())
          {
            jobs = await uow.ProcessService.GetOrderJobDetail(orderId);
          }

          return jobs;
        }

        public async Task<List<AccountTrackModel>> GetOrderAccountTracks(StorageUowProvider storageUowProvider, int orderId)
        {
          var jobs = new List<AccountTrackModel>();

          using (var uow = storageUowProvider.CreateUow())
          {
            jobs = await uow.ProcessService.GetOrderAccountTracks(orderId);
          }

          return jobs;
        }

        public async Task<List<AccountTrackModel>> GetAccountTracks(StorageUowProvider storageUowProvider)
        {
          var jobs = new List<AccountTrackModel>();

          using (var uow = storageUowProvider.CreateUow())
          {
            jobs = await uow.ProcessService.GetAccountTracks();
          }

          return jobs;
        }

        public async Task<ProxyModel[]> GetProxies(StorageUowProvider storageUowProvider)
        {
          var allProxies = new List<ProxyModel>();

          using (var uow = storageUowProvider.CreateUow())
          {
            allProxies = await uow.ProxyStorageService.GetProxies();
          }

          return allProxies.ToArray();
        }

        public async Task<Track[]> GetAccountPlaylist(StorageUowProvider storageUowProvider)
        {
          Track[] accountTracks;

          using (var uow = storageUowProvider.CreateUow())
          {
            accountTracks = await uow.AccountTrackService.GetAccountTracks(_accountId);
            await uow.ApplyChanges();
          }

          return accountTracks;
        }

        public async Task<AccountTrackModel[]> GetAccountTrackModels(StorageUowProvider storageUowProvider, int currentAccountTrackId = 0)
        {
          AccountTrackModel[] accountTracks;

          using (var uow = storageUowProvider.CreateUow())
          {
            accountTracks = await uow.AccountTrackService.GetNextAccountTrackModels(_accountId, currentAccountTrackId);
            await uow.ApplyChanges();
          }

          return accountTracks;
        }

        public async Task<int> GetAccountTracksCount(StorageUowProvider storageUowProvider)
        {
          int accountTracksCount;

          using (var uow = storageUowProvider.CreateUow())
          {
            accountTracksCount = await uow.AccountTrackService.GetAccountTracksCount(_accountId);
            await uow.ApplyChanges();
          }

          return accountTracksCount;
        }

        public async Task SyncPlayList(StorageUowProvider storageUowProvider, Persistence.Track[] newAccountPlaylist)
        {
                /*
          var accountPlaylist = await GetAccountPlaylist(storageUowProvider);


          var playlistDiff = GetDiff(newAccountPlaylist, accountPlaylist);
          using (var uow = storageUowProvider.CreateUow())
          {
            foreach (var track in playlistDiff.TracksToAdd)
            {
              await SpotifySearchPage.Search(_page, track.Title);
              await SpotifySearchPage.ToggleSongPlaylistStatus(_page);
              await uow.AccountTrackService.AddTrack(track.TrackId, track.Title);
            }

            foreach (var track in playlistDiff.TracksToRemove)
            {
              await SpotifySearchPage.Search(_page, track.Title);
              await SpotifySearchPage.ToggleSongPlaylistStatus(_page);
              await uow.AccountTrackService.RemoveTrack(_accountId, track.TrackId);
            }

            await uow.ApplyChanges();
          }
           */
         }

            /// <summary>
            /// Places the order
            /// </summary>
            /// <param name="storageUowProvider"></param>
            /// <param name="orderModel"></param>
            /// <returns></returns>
        public async Task<OrderModel> PlaceOrder(StorageUowProvider storageUowProvider, OrderModel orderModel)
        {
          OrderModel retModel;

          await FillTrackIdInfoIfAvailableAsync(storageUowProvider, orderModel);

          using (var uow = storageUowProvider.CreateUow())
          {
            retModel = await uow.ProcessService.AddOrder(orderModel);
          }

          return retModel;
        }

        /// <summary>
        /// Places the order
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        public async Task<List<OrderModel>> ProcessOrder(StorageUowProvider storageUowProvider)
        {
          var retResult = new List<OrderModel>();
          using (var uow = storageUowProvider.CreateUow())
          {
            retResult = await uow.ProcessService.ProcessOrder();
          }
          return retResult;
        }

        private async Task FillTrackIdInfoIfAvailableAsync(StorageUowProvider storageUowProvider, OrderModel orderModel)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            // uow.ProcessService.AddOrder(orderModel);
            var listOfTracks = await uow.ProcessService.GetTracks(orderModel.Tracks);
            foreach (var track in orderModel.Tracks)
            {
              var trackReceived = listOfTracks.FirstOrDefault(a => a.TrackId == track.TrackId);
              if (trackReceived != null)
              {
                track.Id = trackReceived.Id;
              }
            }
          }
        }

        /// <summary>
        /// Saves proxies to the persistence storage
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="newProxylist"></param>
        /// <returns></returns>
        public async Task AddProxies(StorageUowProvider storageUowProvider, ProxyModel[] newProxylist)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
        
            foreach (var proxyModel in newProxylist)
            {
              var proxyEntity = new Persistence.Proxy();
              proxyEntity.IpAddress = proxyModel.IpAddress;
              proxyEntity.Port = proxyModel.Port;
              proxyEntity.UserName = proxyModel.UserName;
              proxyEntity.Password = proxyModel.Password;
              proxyEntity.Id = proxyModel.Id;
              proxyEntity.Country = proxyModel.Country;
              await uow.ProxyStorageService.AddProxy(proxyEntity);
            }

            await uow.ApplyChanges();
          }
        }

        /// <summary>
        /// Saves accounts to the persistence storage
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="newAccountList"></param>
        /// <returns></returns>
        public async Task AddAccounts(StorageUowProvider storageUowProvider, AccountModel[] newAccountList)
        {
            using (var uow = storageUowProvider.CreateUow())
            {
                foreach (var accountModel in newAccountList)
                {
                    var accountEntity = new Persistence.Account();
                    accountEntity.AccountId = accountModel.AccountId;
                    accountEntity.Email = accountModel.Email;
                    accountEntity.Password = accountModel.Password;
                    accountEntity.Country = accountModel.Country;
                    accountEntity.CurrentProxyId = accountModel.CurrentProxyId;
                    accountEntity.AccountTypeId = accountModel.AccountTypeId;
                    accountEntity.PlayerStatus = accountModel.PlayerStatus;
                    await uow.AccountService.AddAccount(accountEntity);
                }
                await uow.ApplyChanges();            
            }
        }

        /// <summary>
        /// Saves account to the persistence storage
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="newAccountList"></param>
        /// <returns></returns>
        public async Task AddAccount(StorageUowProvider storageUowProvider, AccountModel accountModel)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            var accountEntity = new Account();
            accountEntity.Email = accountModel.Email;
            accountEntity.Password = accountModel.Password;
            accountEntity.Country = accountModel.Country;
            accountEntity.CurrentProxyId = accountModel.CurrentProxyId;
            accountEntity.AccountTypeId = accountModel.AccountTypeId;
            await uow.AccountService.AddAccount(accountEntity);
            await uow.ApplyChanges();
          }
        }

        /// <summary>
        /// Updates account to the persistence storage
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="newAccountList"></param>
        /// <returns></returns>
        public async Task UpdateAccount(StorageUowProvider storageUowProvider, EditAccountModel accountModel)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            await uow.AccountService.UpdateAccount(accountModel);
            await uow.ApplyChanges();
          }
        }

        /// <summary>
        /// Updates account to the persistence storage
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<AccountModel[]> RemoveAccount(StorageUowProvider storageUowProvider, int accountId)
        {
            using (var uow = storageUowProvider.CreateUow())
            {
                //_page.Browser.CloseAsync();
                var account = await uow.AccountService.RemoveAccount(accountId);
                //await uow.AccountStatisticsStorageService.RemoveAccountPlayStatistics(accountId);
                //await uow.ApplyChanges();
                
                return account.ToArray();
            }
        }

        public async Task<AccountListModel> SetAllAccountInfo(StorageUowProvider storageUowProvider, AccountListModel accountList)
        {
            AccountListModel retAccountList;
            using (var uow = storageUowProvider.CreateUow())
            {
                retAccountList = await uow.AccountService.SetAllAccountInfo( accountList);
            }
            return retAccountList;
        }
        public async Task<AccountTypeModel[]> GetAccountTypes(StorageUowProvider storageUowProvider)
        {
          var allAccountTypes = new List<AccountTypeModel>();
          using (var uow = storageUowProvider.CreateUow())
          {
            allAccountTypes = await uow.AccountService.GetAccountTypes();
          }
          return allAccountTypes.ToArray();
        }

        public async Task<AccountCountModel[]> GetAccountCounts(StorageUowProvider storageUowProvider)
        {
            var aCounts = new List<AccountCountModel>();
            using (var uow = storageUowProvider.CreateUow())
            {
                aCounts = await uow.AccountService.GetAccountCounts();
            }
                return aCounts.ToArray();
         }

        public async Task<CountryModel[]> GetCountrys(StorageUowProvider storageUowProvider)
        {
            var countrys = new List<CountryModel>();
            using(var uow = storageUowProvider.CreateUow())
            {
                countrys = await uow.AccountService.GetCountrys();
            }
                return countrys.ToArray();
            }

        public async Task RemoveProxy(StorageUowProvider storageUowProvider, int proxyId)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            await uow.ProxyStorageService.RemoveProxy(proxyId);
            //await uow.AccountStatisticsStorageService.RemoveAccountPlayStatistics(accountId);
            await uow.ApplyChanges();
          }
        }

        /// <summary>
        /// Saves accounts to the persistence storage
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="editAccount"></param>
        /// <returns></returns
        public async Task EditAccounts(StorageUowProvider storageUowProvider, AccountModel newAccountList)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            var accountEntity = new Account();
            accountEntity.Email = newAccountList.Email;
            accountEntity.Password = newAccountList.Password;
            accountEntity.Country = newAccountList.Country;
            accountEntity.CurrentProxyId = newAccountList.CurrentProxyId;
            accountEntity.AccountTypeId = newAccountList.AccountTypeId;
            await uow.AccountService.EditAccount(accountEntity);

            await uow.ApplyChanges();
          }
        }
        /// <summary>
        /// Updates proxy to the persistence storage
        /// </summary>
        /// <param name="storageUowProvider"></param>
        /// <param name="newProxyList"></param>
        /// <returns></returns>
        public async Task UpdateProxy(StorageUowProvider storageUowProvider, ProxyModel proxyModel)
        {
          using (var uow = storageUowProvider.CreateUow())
          {
            await uow.ProxyStorageService.UpdateProxy(proxyModel);
            await uow.ApplyChanges();
          }
        }

        public async Task<DashboardModel[]> GetDashboard(StorageUowProvider storageUowProvider)
        {
            var dashboard = new List<DashboardModel>();
            using (var uow = storageUowProvider.CreateUow())
            {
                dashboard = await uow.ProcessService.GetDashboard();
            }
            return dashboard.ToArray();
        }

        public async Task GetLogStatus(StorageUowProvider storageUowProvider, int accountId)
        {
            var logStatus = "Credential";
            try
            {
                var url = _page.Url;
                //var x = await _page.QuerySelectorAsync("button#login-button");
                if ( url == "https://open.spotify.com/")
                {
                    logStatus = "LoggedIn";
                    var y = await _page.QuerySelectorAsync("div.ConnectBar");
                    if (y != null)
                    {
                        logStatus = "AlreadyInUse";
                    }
                }
                else
                {
                    logStatus = "Credential";
                    var y = await _page.QuerySelectorAllAsync("p.alert");
                    //var y = await _page.QuerySelectorAsync("p.alert");
                    if (y.Length == 0)
                    {
                            logStatus = "Connecting...";
                    }
                    else
                    {
                        var cont = await y[0].EvaluateFunctionAsync<string>("co = > co.innerHTML");
                        if (cont.Contains("Incorrect username or password."))
                        {
                            logStatus = "ValidUserInfo";
                        }
                        else
                        {
                            logStatus = "OopsError";
                        }
                    }
                    /*else
                    {
                        logStatus = "LoggedIn";
                    }*/
                }                               
            }
            catch (Exception e)
            {
                logStatus = "Credental";
            }
            
            using(var uow = storageUowProvider.CreateUow())
            {
                await uow.AccountService.SaveLogStatus(accountId, logStatus);
            }
            return;
        }

        public async Task<ChartModel[]> GetCharts(StorageUowProvider storageUowProvider,int accountNum)
        {
            Track[] trackList;
            using (var uow = storageUowProvider.CreateUow())
            {
                trackList = await uow.TrackStorageService.GetAllTracks();
            }
                
            var data = await SpotifyChartPage.ClickPage(_page,trackList);
            return data.ToArray();
        }
    }
}