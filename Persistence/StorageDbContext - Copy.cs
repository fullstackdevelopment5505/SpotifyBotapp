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

            modelBuilder
                .Entity<AccountTrack>()
                .HasAlternateKey(c => new { c.AccountId, c.TrackId });

            modelBuilder.Entity<Proxy>()
            .HasAlternateKey(a => new { a.IpAddress, a.Port });

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
                Country = "Country A",
                Password = "casperhjorth3005",
                AccountTypeId = 3,
                CurrentProxyId = 1
            }, new Account
            {
                AccountId = 2,
                Email = "athenaeydis@gmail.com",
                Country = "Country B",
                Password = "grimmurkisi",
                AccountTypeId = 3,
                CurrentProxyId = 1
            },
             new Account
             {
                 AccountId = 3,
                 Email = "123marc@live.dk",
                 Country = "Country c",
                 Password = "pomfrit11",
                 AccountTypeId = 3,
                 CurrentProxyId = 1
             });

            modelBuilder.Entity<Track>().HasData(
                new Track { Id = 1, TrackId = "36BTUWJAjywlBVCZuiC6EO", Title = "Hallo" },
                new Track { Id = 2, TrackId = "3ne3QhA3EYW9511Zo9IcY3", Title = "Love moss" },
                new Track { Id = 3, TrackId = "7t5nQByZlNpbPXzYKcf4KD", Title = "vibe" }
                );

            modelBuilder.Entity<AccountTrack>().HasData(new AccountTrack
            {
                Id = 1,
                AccountId = 1,
                TrackId = "36BTUWJAjywlBVCZuiC6EO",
                RequiredPlayCount = 5000,
                PlayCount = 0
            }, new AccountTrack
            {
                Id = 2,
                AccountId = 2,
                TrackId = "3ne3QhA3EYW9511Zo9IcY3",
                RequiredPlayCount = 5000,
                PlayCount = 0
            },
            new AccountTrack
            {
                Id = 3,
                AccountId = 2,
                TrackId = "7t5nQByZlNpbPXzYKcf4KD",
                RequiredPlayCount = 5000,
                PlayCount = 0
            });

            // todo this need to be the id from the Track table

            modelBuilder.Entity<Order>().HasData(new Order
            {
                Id = 1,
                Priority = 1,
                IsActive = true,
                RequiredPlayCount = 15000
            });

            modelBuilder.Entity<Job>().HasData(new Job
            {
                Id = 1,
                OrderId = 1,
                TrackId = 1,
                RequiredPlayCount = 5000
            }, new Job // to be added account tracks for this
            {
                Id = 2,
                OrderId = 1,
                TrackId = 2,
                RequiredPlayCount = 5000
            },
             new Job // to be added account tracks for this
             {
                 Id = 3,
                 OrderId = 1,
                 TrackId = 3,
                 RequiredPlayCount = 5000
             });
        }

        public DbSet<Track> Tracks { get; set; }

        public DbSet<AccountTrack> AccountTracks { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<AccountType> AccountTypes { get; set; }

        public DbSet<AccountTrackPlayStatistics> ProfileTrackPlayStatistics { get; set; }

        public DbSet<Proxy> Proxies { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Job> Jobs { get; set; }

    }
}
