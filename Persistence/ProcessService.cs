using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotifyBot.Api.Model;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;

namespace SpotifyBot.Persistence
{
    public class ProcessService
    {
        readonly StorageDbContext _db;
        private string m_sProxyCountry;
        private string m_sCountry;

        public ProcessService(StorageDbContext db) => _db = db;

        public async Task<List<OrderModel>> GetOrders( int orderId)
        {
            var retResult = new List<OrderModel>();
            var orderQuery = _db.Orders;
            if (orderId > 0)
            {
                orderQuery.Where(o => o.Id == orderId);
            }

            var orders = await orderQuery.Select(o => new OrderModel
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                RequiredPlayCount = o.RequiredPlayCount,
                TrackTitle = "",
                TrackIds = o.TrackIds,
                
                IsProcessed = o.IsProcessed,
                CreatedDate = o.CreatedDate
            }).ToListAsync();
            if (orders.Count != 0)
            {
                var i = 0;
                foreach(var order in orders)
                {
                    var track = await _db.Tracks.Where(t => t.TrackId == order.TrackIds).Select(t => t).SingleAsync();
                    var trackmodel = new TrackModel(track);
                    orders.ElementAt(i).Tracks.Add(trackmodel);
                    orders.ElementAt(i).TrackTitle = track.Title;

                    //var at = await _db.AccountTracks.Where(a => a.TrackId == order.TrackIds).Select(t => t).ToListAsync();
                    //orders.ElementAt(i).AccountCounts = at.Count();
                    var atypes = await _db.AccountTypes.OrderBy(z => z.Id).ToListAsync();
                    orders.ElementAt(i).AccountCounts = new List<int>();
                    orders.ElementAt(i).AccountCounts.Clear();
                    if (atypes.Count > 0)
                    {
                        foreach (var atype in atypes)
                        {
                            var rowcount = new List<AccountTrackModel>();
                            rowcount = await (from at in _db.AccountTracks
                                              join a in _db.Accounts on at.AccountId equals a.AccountId
                                              join o in _db.Jobs on at.JobId equals o.OrderId
                                              where atype.Id == a.AccountTypeId & o.OrderId == order.Id
                                              select new AccountTrackModel
                                              {
                                                  AccountId = a.AccountId,
                                                  Email = a.Email
                                              }).ToListAsync();
                            orders.ElementAt(i).AccountCounts.Add(rowcount.Count());

                        }
                    }
                                      
                    i++;

                }
                
                retResult = orders;
            }

            return retResult;
        }

        public async Task<OrderModel> GetOrderDetail(int orderId)
        {
            var retResult = new OrderModel();
            var orderQuery = _db.Orders.Include(o => o.Jobs);
            //var orderQuery = from o in  _db.Orders.Include(o => o.Jobs )
            //                  join t in _db.Tracks on o.Jobs.
            if (orderId > 0)
            {
                orderQuery.Where(o => o.Id == orderId);
            }

            var order = await orderQuery.Select(o => new OrderModel
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                RequiredPlayCount = o.RequiredPlayCount,
                TrackIds = o.TrackIds,
                IsProcessed = o.IsProcessed,
                Jobs = GetJobModels(o.Jobs)
            }).SingleOrDefaultAsync();

            if (order != null)
            {
                retResult = order;
            }

            return retResult;
        }


        public async Task<List<JobModel>> GetOrderJobDetail(int orderId)
        {
            var retResult = new List<JobModel>();
            var jobs = await (from j in _db.Jobs
                              join t in _db.Tracks on j.TrackId equals t.Id
                              where j.OrderId == orderId
                              select new
                              {
                                  JobDetail = j,
                                  TrackDetail = t
                              }).ToListAsync();

            foreach (var jobTrackGroup in jobs)
            {
                var jobModel = new JobModel(jobTrackGroup.JobDetail.OrderId);

                jobModel.TrackId = jobTrackGroup.TrackDetail.Id;
                jobModel.Id = jobTrackGroup.JobDetail.Id;
                jobModel.OrderId = jobTrackGroup.JobDetail.OrderId;
                jobModel.RequiredPlayCount = jobTrackGroup.JobDetail.RequiredPlayCount;
                jobModel.Track = new TrackModel
                {
                    TrackId = jobTrackGroup.TrackDetail.TrackId,
                    TrackTitle = jobTrackGroup.TrackDetail.Title
                };
                retResult.Add(jobModel);
            }

            return retResult;
        }

        public async Task<List<AccountTrackModel>> GetOrderAccountTracks(int orderId)
        {
            var retResult = new List<AccountTrackModel>();
            var jobIds = _db.Jobs.Where(o => o.OrderId == orderId).Select(j=> j.Id);
            var accountTrackModels = await ( 
                              from at in _db.AccountTracks  
                              join a in _db.Accounts on at.AccountId equals a.AccountId
                              where jobIds.Contains( at.JobId) 
                              select new AccountTrackModel
                              {
                                  AccountId = at.AccountId,
                                  Email = a.Email,
                                  TrackId = at.TrackId,
                                  JobId = at.JobId,
                                  RequiredPlayCount = at.RequiredPlayCount,
                                  PlayCount = at.PlayCount
                              }).ToListAsync();

            retResult = accountTrackModels;
            return retResult;
        }

        public async Task<List<AccountTrackModel>> GetAccountTracks(int orderId = 0)
        {
            var retResult = new List<AccountTrackModel>();
            var accountTrackModels = await (
                              from at in _db.AccountTracks
                              join a in _db.Accounts on at.AccountId equals a.AccountId
                              select new AccountTrackModel
                              {
                                  AccountId = at.AccountId,
                                  Email = a.Email,
                                  TrackId = at.TrackId,
                                  JobId = at.JobId,
                                  RequiredPlayCount = at.RequiredPlayCount,
                                  PlayCount = at.PlayCount
                              }).ToListAsync();

            retResult = accountTrackModels;
            return retResult;
        }

        private List<JobModel> GetJobModels(List<Job> jobs)
        {
            var retResult = new List<JobModel>();
            foreach (var job in jobs)
            {
                retResult.Add(new JobModel(job.OrderId)
                {
                    Id = job.Id,
                    TrackId = job.TrackId,
                    OrderId = job.OrderId,
                    RequiredPlayCount = job.RequiredPlayCount
                });
            }

            return retResult;
        }

        public async Task<List<OrderModel>> ProcessOrder()
        {
            var retResult = false;
           
            var orderEntity = await _db.Orders.Where(j => j.IsProcessed == false).ToListAsync();
            if (orderEntity.Count != 0)
            {
                var accounts = await _db.Accounts.Where(a => a.PlayerStatus != "Credentail" && a.PlayerStatus != "ProxyAddressFail").ToListAsync();
                var totalAccounts = accounts.Count();
                var accountTrackEntity = new AccountTrack();
                var toBeAddedEntities = new List<AccountTrack>();
                foreach (var order in orderEntity)
                {
                    var jobs = await _db.Jobs.Include(j => j.Track).Where(j => j.OrderId == order.Id).ToListAsync();
                    foreach (var job in jobs)
                    {
                        double actualCount = (double)job.RequiredPlayCount / totalAccounts;
                        var playCountRounded = GetRoundedValue(actualCount);

                        foreach (var account in accounts)
                        {
                            accountTrackEntity = new AccountTrack();
                            accountTrackEntity.AccountId = account.AccountId;
                            accountTrackEntity.TrackId = job.Track.TrackId;
                            accountTrackEntity.JobId = job.Id;
                            accountTrackEntity.RequiredPlayCount = playCountRounded;
                            accountTrackEntity.PlayCount = 0;
                            toBeAddedEntities.Add(accountTrackEntity);
                        }
                    }
                    _db.AccountTracks.AddRange(toBeAddedEntities);
                    order.IsProcessed = true;
                    var resultCount = await _db.SaveChangesAsync();
                    retResult = resultCount > 0;
                }
            }

            var orderList = new List<OrderModel>();
            var orderQuery = _db.Orders;
            var orders = await orderQuery.Select(o => new OrderModel
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                RequiredPlayCount = o.RequiredPlayCount,
                TrackTitle = "",
                TrackIds = o.TrackIds,
                AccountCounts = new List<int>(),
                IsProcessed = o.IsProcessed,
                CreatedDate = o.CreatedDate
            }).ToListAsync();
            if (orders.Count != 0)
            {
                var i = 0;
                foreach (var order in orders)
                {
                    var track = await _db.Tracks.Where(t => t.TrackId == order.TrackIds).Select(t => t).SingleAsync();
                    var trackmodel = new TrackModel(track);
                    orders.ElementAt(i).Tracks.Add(trackmodel);
                    orders.ElementAt(i).TrackTitle = track.Title;
                    var atypes = await _db.AccountTypes.OrderBy(z => z.Id).ToListAsync();
                    orders.ElementAt(i).AccountCounts = new List<int>();
                    orders.ElementAt(i).AccountCounts.Clear();
                    if (atypes.Count > 0)
                    {
                        foreach (var atype in atypes)
                        {
                            var rowcount = new List<AccountTrackModel>();
                            rowcount = await (from at in _db.AccountTracks
                                              join a in _db.Accounts on at.AccountId equals a.AccountId
                                              join o in _db.Jobs on at.JobId equals o.OrderId
                                              where atype.Id == a.AccountTypeId & o.OrderId == order.Id
                                              select new AccountTrackModel
                                              {
                                                  AccountId = a.AccountId,
                                                  Email = a.Email
                                              }).ToListAsync();
                            orders.ElementAt(i).AccountCounts.Add(rowcount.Count());
                        }
                    }

                    i++;

                }

                orderList = orders;
            }

            return orderList;

        }

        private int GetRoundedValue(double input)
        {
            return Convert.ToInt32(Math.Ceiling(input));
        }

        public async Task<OrderModel> AddOrder(OrderModel orderModel)
        {
            // here we have checked in the frontend that tracks are present
            double actualCount = (double)orderModel.RequiredPlayCount / orderModel.Tracks.Count();

            var playCountRounded = GetRoundedValue(actualCount);
            DateTime dateTime = DateTime.UtcNow.Date;
            var orderEntity = new Order()
            {
                Title = orderModel.Title,
                Description = orderModel.Description,
                TrackIds = orderModel.TrackIds,
                RequiredPlayCount = orderModel.RequiredPlayCount,
                Priority = orderModel.Priority,
                IsActive = orderModel.IsActive,
                CreatedDate = dateTime.ToString("dd-MM-yyyy")
            };

            await _db.Orders.AddAsync(orderEntity);

            var listOfJobEntities = new List<Job>();
            foreach (var track in orderModel.Tracks)
            {
                var jobEntity = new Job();
                jobEntity.TrackId = track.Id;
                jobEntity.OrderId = orderEntity.Id;
                jobEntity.RequiredPlayCount = playCountRounded;
                listOfJobEntities.Add(jobEntity);
                await _db.Jobs.AddAsync(jobEntity);
            }

            await _db.SaveChangesAsync();

            orderModel.Id = orderEntity.Id;
            orderModel.Jobs = GetJobModelsFromEntity(listOfJobEntities, orderEntity.Id);
            return orderModel;
        }

        private List<JobModel> GetJobModelsFromEntity(List<Job> listOfJobEntities, int orderId)
        {
            var retModels = new List<JobModel>();
            foreach (Job jobEntity in listOfJobEntities)
            {
                var jobModel = new JobModel(orderId);
                jobModel.Id = jobEntity.Id;
                jobModel.OrderId = jobEntity.OrderId;
                jobModel.TrackId = jobEntity.TrackId;
                jobModel.RequiredPlayCount = jobEntity.RequiredPlayCount;
                retModels.Add(jobModel);
            }

            return retModels;
        }

        //public async Task AddAccount(Account accountEntity)
        //{
        //    await _db.Accounts.AddAsync(accountEntity);
        //}

        //public async Task UpdateAccount(AccountModel accountModel)
        //{
        //    var account = _db.Accounts.FirstOrDefault(x => x.AccountId == accountModel.AccountId);
        //    if (account != null)
        //    {
        //        account.AccountId = accountModel.AccountId;
        //        account.Password = accountModel.Password;
        //        account.Country = accountModel.Country;
        //        account.CurrentProxyId = accountModel.CurrentProxyId;

        //        //await _db.Accounts.AddAsync(account);
        //    }
        //}

        //public async Task RemoveAccount(int accountId)
        //{
        //    var account = _db.Accounts.FirstOrDefault(x => x.AccountId == accountId);
        //    if (account != null) _db.Accounts.Remove(account);
        //}

        //public async Task<List<AccountModel>> GetAccounts()
        //{
        //    var accounts = await (from a in _db.Accounts
        //                          join at in _db.AccountTypes on a.AccountTypeId equals at.Id
        //                          join p in _db.Proxies on a.CurrentProxyId equals p.Id
        //                          // where a.AccountId == accountId && a.Status = "Active"
        //                          select new AccountModel
        //                          {
        //                              CurrentProxy = new ProxyModel(p.IpAddress, p.Port, p.Id),
        //                              Email = a.Email,
        //                              Password = a.Password,
        //                              Country = a.Country,
        //                              AccountId = a.AccountId,
        //                              AccountType = at.Type,
        //                              AccountTypeId = a.AccountTypeId,
        //                              CurrentProxyId = a.CurrentProxyId
        //                          })
        //                          // .Take(2)
        //                          .ToListAsync();
        //    return accounts;

        //}

        public async Task<List<TrackModel>> GetTracks(List<TrackModel> trackModels)
        {
            var listOfTrackIds = trackModels.Select(t => t.TrackId);
            var trackEntities = await _db.Tracks
                                    .Where(r => listOfTrackIds.Contains(r.TrackId))
                                    .Select(t => t).ToListAsync();

            var trackEntityIds = trackEntities.Select(t => t.TrackId);

            var toBeCreatedTracks = trackModels.Where(r => !trackEntityIds.Contains(r.TrackId)).ToList();

            var newTracksCreated = new List<Track>();
            foreach (var track in toBeCreatedTracks)
            {
                var trackEntity = new Track()
                {
                    TrackId = track.TrackId,
                    Title = track.TrackTitle,
                };

                newTracksCreated.Add(trackEntity);

                _db.Tracks.Add(trackEntity);
            }

            await _db.SaveChangesAsync();

            trackEntities.AddRange(newTracksCreated);

            return trackEntities.Select(t => new TrackModel
            {
                Id = t.Id,
                TrackId = t.TrackId,
                TrackTitle = t.Title
            }).ToList();
        }

        public async Task AddJob(JobModel jobModel)
        {
            var jobEntity = new Job
            {
                OrderId = jobModel.OrderId,
                TrackId = jobModel.TrackId,
                RequiredPlayCount = jobModel.RequiredPlayCount,
            };

            await _db.Jobs.AddAsync(jobEntity);
        }
        public async Task<List<DashboardModel>> GetDashboard()
        {
            var dashboards = new List<DashboardModel>();
            var datas = new List<DashboardModel>();
            var dashboard = new DashboardModel();
            dashboards = await (
                              from at in _db.Orders
                              join a in _db.Tracks on at.TrackIds equals a.TrackId
                              join b in _db.AccountTracks on at.TrackIds equals b.TrackId
                              join c in _db.Accounts on b.AccountId equals c.AccountId
                              join d in _db.AccountTypes on c.AccountTypeId equals d.Id
                              join e in _db.Proxies on c.CurrentProxyId equals e.Id
                              //join f in _db.Countries on c.Country equals f.CountryName
                              
                              select new DashboardModel
                              {
                                OrderId = at.Id,
                                OrderTitle = at.Title,
                                OrderDescription = at.Description,
                                AccountId = c.AccountId,
                                Username = c.Email,
                                Country = c.Country,
                                AccountType = d.Type,
                                ProxyIpAddress = e.IpAddress,
                                ProxyCountry = e.Country,                              
                                JobId = b.JobId,
                                CreatedDate = at.CreatedDate,
                                Songs = a.Title,
                                Status = c.PlayerStatus,
                                AllPlays = b.RequiredPlayCount,
                                CurrentPlays = b.PlayCount,
                                AccountCounts = 1
                              }).ToListAsync();
            if (dashboards.Count == 0) return dashboards;
            int oId = dashboards[0].OrderId; int acounts = 1; int k = 0;int datak = 0;
            int allplays_sum = 0;
            int cuplays_sum = 0;


            for (int i = 1; i < dashboards.Count(); i++)
            {
                
                if (oId == dashboards[i].OrderId)
                {
                    acounts++;
                }
                else
                {
                    dashboard = new DashboardModel();
                    dashboard.OrderId = dashboards[k].OrderId;
                    dashboard.OrderTitle = dashboards[k].OrderTitle;
                    dashboard.OrderDescription = dashboards[k].OrderDescription;
                    dashboard.JobId = dashboards[k].JobId;
                    dashboard.CreatedDate = dashboards[k].CreatedDate;
                    dashboard.Songs = dashboards[k].Songs;
                    dashboard.AccountCounts = acounts;
                    dashboard.Status = "-";
                    dashboards[k].AccountCounts = 0;//set as 0                    
                    allplays_sum = dashboards[k].AllPlays;
                    cuplays_sum = dashboards[k].CurrentPlays;
                    datas.Add(dashboard);
                    datas.Add(dashboards[k]);
                    for (int j = k + 1; j < k + acounts; j++)
                    {
                        dashboards[j].AccountCounts = -1 * (j - k);
                        allplays_sum = allplays_sum + dashboards[j].AllPlays;
                        cuplays_sum = cuplays_sum + dashboards[j].CurrentPlays;
                        datas.Add(dashboards[j]);
                    }
                    datas[datak].AllPlays = allplays_sum;
                    datas[datak].CurrentPlays = cuplays_sum;
                    oId = dashboards[k + acounts].OrderId;
                    k = k + acounts;
                    datak = datak + acounts + 1;
                    acounts = 1;
                }
            }
            dashboard = new DashboardModel();
            dashboard.OrderId = dashboards[k].OrderId;
            dashboard.OrderTitle = dashboards[k].OrderTitle;
            dashboard.OrderDescription = dashboards[k].OrderDescription;
            dashboard.JobId = dashboards[k].JobId;
            dashboard.CreatedDate = dashboards[k].CreatedDate;
            dashboard.Songs = dashboards[k].Songs;
            dashboard.AccountCounts = acounts;
            dashboard.Status = "-";
            datas.Add(dashboard);
            dashboards[k].AccountCounts = 0;
            datas.Add(dashboards[k]);

            allplays_sum = dashboards[k].AllPlays;
            cuplays_sum = dashboards[k].CurrentPlays;
            for (int i = k + 1; i < k + acounts; i++)
            {
                dashboards[i].AccountCounts = -1 * (i - k);
                allplays_sum = allplays_sum + dashboards[i].AllPlays;
                cuplays_sum = cuplays_sum + dashboards[i].CurrentPlays;
                datas.Add( dashboards[i]);
            }
            datas[datak].AllPlays = allplays_sum;
            datas[datak].CurrentPlays = cuplays_sum;
            return datas;
        }
        
        public string GetUserCountryByIp(string ip)
        {
            m_sProxyCountry = "";
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
        
        public async void GetCountryAbbreviation(string proxyCountry)
        {
            var country = await _db.Countries.Where(pc => pc.CountryName == proxyCountry).SingleAsync();
            m_sProxyCountry = country.CountryCode;
        }
    }
}