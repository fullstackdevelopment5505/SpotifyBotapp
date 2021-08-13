using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SpotifyBot.Api.Model;
using SpotifyBot.Persistence;
using SpotifyBot.Shared;

namespace SpotifyBot.Api
{
    [ApiController]
    [Route("/accounts/{accountId}")]
    [EnableCors("AllowAll")]
    public sealed class AccountController : ControllerBase
    {

        public static SpotifyServiceGroup das = null;
        public static StorageUowProvider provide = null;
        [Route("start-playing/{orderId}")]
        [HttpPost]
        public async Task<AccountPlayingResponse> StartPlaying(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId,int orderId)
        {
            AccountPlayingResponse accountPlayingResponseResult;
            
            accountPlayingResponseResult = new AccountPlayingResponse();
            SpotifyServiceGroup _serviceGroup = das != null ? das : spotifyServiceGroup;
            var spotifyMainService = _serviceGroup.GetMainService();
            provide = provide != null ? provide : storageUowProvider;
            var accounts = await spotifyMainService.GetOrderAccounts(provide, orderId, accountId);
            accountPlayingResponseResult.orderId = orderId;
            if (accounts.Length <= 0)
            {
                accountPlayingResponseResult.IsPlaying = false;
            }
            else
            {
                foreach (var account in accounts)
                {
                    var spotifyService = _serviceGroup.GetService(account.AccountId);

                    spotifyService.StartParticularTracks(provide);
                    accountPlayingResponseResult.States.Add(new AccountState()
                    {
                        AccountId = account.AccountId,
                        Email = account.Email,
                        IsPlaying = spotifyService.IsPlaying(),
                        PlayerStatus = spotifyService.IsPlaying() ? "Playing" : "Complete"
                    });
                    spotifyService.SetPlayCount(provide, orderId, account.AccountId);
                }
                SetPlayingState(accountPlayingResponseResult);
            }           
            
                      
            return accountPlayingResponseResult;

        }
        [Route("stop-playing/{orderId}")]
        [HttpPost]
        public async Task<AccountPlayingResponse> StopPlayingAsync([FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId,int orderId)
        {
            var accountPlayingResponseResult = new AccountPlayingResponse();
            var spotifyMainService = spotifyServiceGroup.GetMainService();
            var accounts = await spotifyMainService.GetOrderAccounts(provide, orderId, accountId);

            foreach (var account in accounts)
            {
                var spotifyService = spotifyServiceGroup.GetService(account.AccountId);

                spotifyService.UpdateAccountPlayingStatus(storageUowProvider, "Stopped");
                spotifyService.StopPlaylist();
                accountPlayingResponseResult.States.Add(new AccountState()
                {
                    AccountId = account.AccountId,
                    Email = account.Email,
                    IsPlaying = spotifyService.IsPlaying(),
                    PlayerStatus = "Stopped"
                });
            }
            accountPlayingResponseResult.orderId = orderId;
            return accountPlayingResponseResult;
        }

        [Route("sync-playlist")]
        [HttpPost]
        public async Task<JsonResult> SyncPlayList(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId, SyncAccountPlaylistRequest playList)
        {
            var spotifyService = spotifyServiceGroup.GetService(accountId);
            await spotifyService.SyncPlayList(storageUowProvider, playList.Playlist.Tracks);

            return new JsonResult(new { status = "synchronized" });
        }

        [Route("place-order")]
        [HttpPost]
        // todo to work on
        public async Task<JsonResult> PlaceOrder(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId, OrderModel orderRequest)
        {
            var spotifyService = spotifyServiceGroup.GetMainService();
            await spotifyService.PlaceOrder(storageUowProvider, orderRequest);

            return new JsonResult(new { status = "synchronized" });
        }


        [Route("get-state")]
        public async Task<AccountPlayingResponse> GetState(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
             [FromServices] StorageUowProvider storageUowProvider, int accountId)
        {
            var accountStateResponse = new AccountPlayingResponse();
            accountStateResponse.IsPlaying = true;
            var spotifyMainService = spotifyServiceGroup.GetService(accountId);
            
            var accounts = await spotifyMainService.GetAccounts(storageUowProvider);
            await spotifyServiceGroup.GetLogStatus(storageUowProvider);
            foreach (var account in accounts)
            {
                var spotifyService = spotifyServiceGroup.GetService(account.AccountId);

                accountStateResponse.States.Add(new AccountState()
                {
                    AccountId = account.AccountId,
                    Email = account.Email,
                    IsPlaying = spotifyService.IsPlaying(),
                    PlayerStatus = account.PlayerStatus
                });
            }

            SetPlayingState(accountStateResponse);

            return accountStateResponse;
        }

        private void SetPlayingState(AccountPlayingResponse accountStateResponse)
        {
            accountStateResponse.IsPlaying = false;

            if (accountStateResponse.States.FindIndex(s => s.IsPlaying) >= 0)
            {
                accountStateResponse.IsPlaying = true;
            }
        }

        [Route("get-playlist")]
        public async Task<GetAccountPlaylistResponse> GetAccountPlaylist(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId)
        {
            //Thread.Sleep(10000);
            var spotifyService = spotifyServiceGroup.GetService(accountId);
            var accountTracks = await spotifyService.GetAccountPlaylist(storageUowProvider);
            return new GetAccountPlaylistResponse() { Playlist = new AccountPlaylist() { Tracks = accountTracks } };
        }

        [Route("add-track")]
        [HttpPost]
        public async Task<JsonResult> AddTrack(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId, [FromBody] Model.TrackModel track)
        {
            var spotifyService = spotifyServiceGroup.GetService(accountId);
            await spotifyService.AddTrack(storageUowProvider, accountId, track.TrackId, track.TrackTitle);

            return new JsonResult(new { status = "added" });
        }

        [Route("remove-track/{orderId}")]
        public async Task<JsonResult> RemoveTrack(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId, int orderId)
        {
            var spotifyService = spotifyServiceGroup.GetService(accountId);
            await spotifyService.RemoveTrack(storageUowProvider, orderId);

            return new JsonResult(new { status = "removed" });
        }

        [Route("edit-track/{orderId}")]
        [HttpPost]
        public async Task<JsonResult> UpdateTrack(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId, int orderId, NewTrackDataModel newTrackData)
        {
            var spotifyService = spotifyServiceGroup.GetService(accountId);
            await spotifyService.EditTrack(storageUowProvider, orderId, newTrackData);

            return new JsonResult(new { status = "updated" });
        }

        [Route("get-statistics")]
        public async Task<GetAccountStatisticsResponse> GetStatistics(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId)
        {
            var spotifyService = spotifyServiceGroup.GetMainService();
            var tracksStatistic = await spotifyService.GetStatistics(storageUowProvider);
            return new GetAccountStatisticsResponse() { Statistics = tracksStatistic };
        }

        [Route("start-chrome")]
        [HttpPost]
        public async Task<IActionResult> ChromeStart(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
           [FromServices] StorageUowProvider storageUowProvider,
           int accountId, AccountListModel accountList)
        {
            
            var spotifyService = spotifyServiceGroup.GetService(accountId);
            var config = await SpotifyAccountsConfig.Read();
            spotifyServiceGroup = das != null ? das : spotifyServiceGroup;
            AccountListModel accountListModel = await spotifyService.SetAllAccountInfo(storageUowProvider, accountList);
            SpotifyServiceGroup addspotifyServiceGroup = await SpotifyServiceGroup.Create(storageUowProvider, config, accountListModel.Accounts,spotifyServiceGroup);
            
            return new JsonResult(new { status = "success" });
        }

        [Route("add-accounts")]
        [HttpPost]
        public async Task<IActionResult> SaveAccounts(
           [FromServices] SpotifyServiceGroup spotifyServiceGroup,
           [FromServices] StorageUowProvider storageUowProvider,
           int accountId, AccountListModel accountList)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetService(accountId);
                await spotifyService.AddAccounts(storageUowProvider, accountList.Accounts);
                /*
                var config = await SpotifyAccountsConfig.Read();
                AccountListModel accountListModel = await spotifyService.SetAllAccountInfo(storageUowProvider, accountList);
                var addSpotifyServiceGroup = await SpotifyServiceGroup.Create(storageUowProvider, config, accountListModel.Accounts);
                await addSpotifyServiceGroup.GetAddAccountLogStatus(storageUowProvider, accountListModel.Accounts);
                */
                return new JsonResult(new { status = "Saved" });
            }

            catch (Exception ex)
            {
                // Log.Error(excep);
                return StatusCode(500, ex.Message);
            }
        }

        [Route("add-account")]
        [HttpPost]
        public async Task<IActionResult> SaveAccount(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            [FromBody] AccountModel account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {

                var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
                await spotifyService.AddAccount(storageUowProvider, account);

                return new JsonResult(new { status = "Saved" });
            }

            catch (Exception excep)
            {
                // Log.Error(excep);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Updates account
        /// </summary>
        /// <param name="spotifyServiceGroup"></param>
        /// <param name="storageUowProvider"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        [Route("update-account")]
        [HttpPost]
        public async Task<IActionResult> UpdateAccount(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            [FromBody] EditAccountModel account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
                await spotifyService.UpdateAccount(storageUowProvider, account);

                return new JsonResult(new { status = "Updated" });
            }

            catch (Exception excep)
            {
                // Log.Error(excep);
                return StatusCode(500);
            }
        }

        [Route("delete-account")]
        [HttpDelete]
        public async Task<GetAccountResponse> RemoveAccount(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int accountId)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
                var account = await spotifyService.RemoveAccount(storageUowProvider, accountId);
                return new GetAccountResponse() { Accounts = account };
            }

            catch (Exception excep)
            {
                // Log.Error(excep);
                //return StatusCode(500);
                return null;
            }
        }

        [Route("get-account-types")]
        public async Task<AccountTypeResponse> GetAccountTypes(
        [FromServices] SpotifyServiceGroup spotifyServiceGroup,
        [FromServices] StorageUowProvider storageUowProvider)
        {
            var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
            var accountTypes = await spotifyService.GetAccountTypes(storageUowProvider);
            return new AccountTypeResponse() { AccountTypes = accountTypes };
        }

        [Route("delete-proxies/{proxyId}")]
        public async Task<IActionResult> DeleteProxies(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            int proxyId)
        {
            try
            {
                var isProxyInUsed = await spotifyServiceGroup.IsAccountLinkedWithProxyId(storageUowProvider, proxyId);

                if (!isProxyInUsed)
                {
                    var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
                    await spotifyService.RemoveProxy(storageUowProvider, proxyId);

                    return new JsonResult(new { status = "removed_proxy" });
                }

                return StatusCode(500);
            }

            catch (Exception excep)
            {
                // Log.Error(excep);
                return StatusCode(500);
            }

        }

        [Route("edit-accounts")]
        [HttpPut]
        public async Task<IActionResult> EditAccount(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
            [FromBody] EditAccountModel account)
        {

            try
            {
                var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
                await spotifyService.UpdateAccount(storageUowProvider, account);

                return new JsonResult(new { status = "Saved" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [Route("get-proxies")]
        public async Task<GetProxyResponse> GetProxies(
          [FromServices] SpotifyServiceGroup spotifyServiceGroup,
          [FromServices] StorageUowProvider storageUowProvider)
        {
            var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
            var proxies = await spotifyService.GetProxies(storageUowProvider);
            return new GetProxyResponse() { Proxies = proxies };
        }

        [Route("add-proxies")]
        [HttpPost]
        public async Task<IActionResult> SaveProxies(
           [FromServices] SpotifyServiceGroup spotifyServiceGroup,
           [FromServices] StorageUowProvider storageUowProvider,
           int accountId, ProxyListModel proxyList)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetService(accountId);
                await spotifyService.AddProxies(storageUowProvider, proxyList.Proxies);

                return new JsonResult(new { status = "Saved" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }

        }


        [Route("edit-proxies")]
        [HttpPut]
        public async Task<IActionResult> EditProxy(
              [FromServices] SpotifyServiceGroup spotifyServiceGroup,
              [FromServices] StorageUowProvider storageUowProvider,
              [FromBody] ProxyModel proxyList,int accountId)
        {

            try
            {
                var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
                await spotifyService.UpdateProxy(storageUowProvider, proxyList);

                return new JsonResult(new { status = "Saved" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Route("get-accounts")]
        public async Task<GetAccountResponse> GetAccounts(
           [FromServices] SpotifyServiceGroup spotifyServiceGroup,
           [FromServices] StorageUowProvider storageUowProvider)
        {
            var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
            var accounts = await spotifyService.GetAccounts(storageUowProvider);
            return new GetAccountResponse() { Accounts = accounts };
        }

        [Route("get-account-info")]
        public async Task<AccountModel> GetAccountInfo(
           [FromServices] SpotifyServiceGroup spotifyServiceGroup,
           [FromServices] StorageUowProvider storageUowProvider,
          int accountId)
        {
            var spotifyService = spotifyServiceGroup.GetService(accountId);
            var accountInfo = await spotifyService.GetAccountInfo(storageUowProvider,accountId);
            return accountInfo;
        }

        [Route("get-account-counts")]
        public async Task<GetAccountCountResponse> GetAccountCounts(
           [FromServices] SpotifyServiceGroup spotifyServiceGroup,
           [FromServices] StorageUowProvider storageUowProvider)
        {
            var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
            var accounts = await spotifyService.GetAccountCounts(storageUowProvider);
            return new GetAccountCountResponse() { AccountCounts = accounts };
        }

        [Route("get-countrys")]
        public async Task<CountryModel[]> GetCountrys(
           [FromServices] SpotifyServiceGroup spotifyServiceGroup,
           [FromServices] StorageUowProvider storageUowProvider)
        {
            var spotifyService = spotifyServiceGroup.GetService(AppConstants.Configurations.MasterAccountId);
            var accounts = await spotifyService.GetCountrys(storageUowProvider);
            return accounts;
        }


    }
}