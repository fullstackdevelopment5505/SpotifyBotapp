using Microsoft.EntityFrameworkCore;

namespace SpotifyBot.Persistence
{
    public sealed class StorageDbContext : DbContext
    {
        public StorageDbContext(DbContextOptions opts) : base(opts) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<AccountTrackPlayStatistics>()
                .HasKey(c => new { c.AccountId, c.TrackId });

            //modelBuilder
            //    .Entity<AccountTrack>()
            //    .HasAlternateKey(c => new { c.AccountId, c.TrackId });

            //modelBuilder.Entity<Proxy>()
            //.HasAlternateKey(a => new { a.IpAddress, a.Port });

            modelBuilder.Entity<Account>()
           .HasAlternateKey(a => new { a.Email });

            modelBuilder.Entity<AccountType>().HasData(new AccountType { Id = 1, Type = "Premium" },
                new AccountType { Id = 2, Type = "Family Member" },
                new AccountType { Id = 3, Type = "Family Owner" },
                new AccountType { Id = 4, Type = "Free" });

            modelBuilder.Entity<Proxy>().HasData(new Proxy { Id = 1, IpAddress = "p.webshare.io", Port = 20001 });

            modelBuilder.Entity<Account>().HasData(new Account
            {
                AccountId = 1,
                Email = "chh@hjorth-hansen.dk",
                Country = "Germany",
                Password = "casperhjorth3005",
                AccountTypeId = 3,
                CurrentProxyId = 1,
                PlayerStatus = "Stoped"
            }, new Account
            {
                AccountId = 2,
                Email = "athenaeydis@gmail.com",
                Country = "Germany",
                Password = "grimmurkisi",
                AccountTypeId = 3,
                CurrentProxyId = 1,
                PlayerStatus = "Stoped"
            }
            );

            modelBuilder.Entity<Track>().HasData(
                new Track { Id = 1, TrackId = "5KLLla01QDKynKYCwA21IX", Title = "Hallo" },
                new Track { Id = 2, TrackId = "0RBtOSR6zumLH1K72gjKjX", Title = "Love moss" },
                new Track { Id = 3, TrackId = "7avivRos1r7tqx2CXIffxP", Title = "vibe" }
                );

            /*modelBuilder.Entity<Country>().HasData(new Country {
                    Id = 1,
                    CountryName = "Germany",
                    CountryCode = "de"
                }  ,new Country {
                    Id = 2,
                    CountryName = "China",
                    CountryCode = "cn"
                }
            );*/
            // todo this need to be the id from the Track table

            //modelBuilder.Entity<Order>().HasData(new Order
            //{
            //    Id = 1,
            //    Title = "Order title 1",
            //    Description = "Oder Desc 1", 
            //    TrackIds= "7wMq5n8mYSKlQIGECKUgTX",
            //    Priority = 1,
            //    IsProcessed = false,
            //    IsActive = true,
            //    RequiredPlayCount = 1000
            //});

            //modelBuilder.Entity<Job>().HasData(new Job
            //{
            //    Id = 1,
            //    OrderId = 1,
            //    TrackId = 1,
            //    RequiredPlayCount = 500
            //}, new Job  
            //{
            //    Id = 2,
            //    OrderId = 1,
            //    TrackId = 2,
            //    RequiredPlayCount = 500
            //});


            //modelBuilder.Entity<AccountTrack>().HasData(new AccountTrack
            //{
            //    Id = 1,
            //    AccountId = 1,
            //    JobId = 1,
            //    TrackId = "5KLLla01QDKynKYCwA21IX",
            //    RequiredPlayCount = 50,
            //    PlayCount = 0
            //}, new AccountTrack
            //{
            //    Id = 2,
            //    AccountId = 2,
            //    JobId = 1,
            //    TrackId = "5KLLla01QDKynKYCwA21IX",
            //    RequiredPlayCount = 50,
            //    PlayCount = 0
            //});

        }

        public DbSet<Track> Tracks { get; set; }

        public DbSet<AccountTrack> AccountTracks { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<AccountType> AccountTypes { get; set; }

        public DbSet<AccountTrackPlayStatistics> ProfileTrackPlayStatistics { get; set; }

        public DbSet<Proxy> Proxies { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<Country> Countries { get; set; }

    }
}
