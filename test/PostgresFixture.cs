using KidFit.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace KidFit.Tests
{
    public class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container;

        public string ConnectionString => _container.GetConnectionString();

        public PostgresFixture()
        {
            _container = new PostgreSqlBuilder("postgres:18.1-alpine3.22")
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            using var context = new AppDbContext(options);

            // Apply real migrations
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }
    }
}

