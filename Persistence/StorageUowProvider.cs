using Microsoft.EntityFrameworkCore;

namespace SpotifyBot.Persistence
{
    public class StorageUowProvider
    {
        readonly DbContextOptions _dbContextOptions;

        public StorageUowProvider(DbContextOptions dbContextOptions) =>
            _dbContextOptions = dbContextOptions;


        public StorageUow CreateUow() =>
            new StorageUow(new StorageDbContext(_dbContextOptions));

        public static StorageUowProvider Init()
        {
            const string connStr = "Data Source=data.sqlite3;";
            var opts = new DbContextOptionsBuilder()
                .UseSqlite(connStr)
                .Options;
            var ctx = new StorageDbContext(opts);
            ctx.Database.EnsureCreated();
            return new StorageUowProvider(opts);
        }
    }
}
