using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Queries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KidFit.Tests.Repositories
{
    public class GenericRepoTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly GenericRepo<Module> _repo;

        public GenericRepoTests()
        {
            // Use SQLite in-memory database for repository tests
            // This supports ExecuteUpdateAsync which InMemory provider does not
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
            _repo = new GenericRepo<Module>(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        [Fact]
        public async Task CreateAsync_AddsEntityToDatabase()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            await _repo.CreateAsync(module);
            await _context.SaveChangesAsync();

            var result = await _context.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
            Assert.NotNull(result);
            Assert.Equal("Test Module", result.Name);
        }

        [Fact]
        public async Task CreateBatchAsync_AddsMultipleEntities()
        {
            var modules = new List<Module>
            {
                new() { Id = Guid.NewGuid(), Name = "Module 1", Description = "Desc 1", CoreSlot = 1, TotalSlot = 2 },
                new() { Id = Guid.NewGuid(), Name = "Module 2", Description = "Desc 2", CoreSlot = 3, TotalSlot = 4 }
            };

            await _repo.CreateBatchAsync(modules);
            await _context.SaveChangesAsync();

            var count = await _context.Modules.CountAsync();
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            await _context.Modules.AddAsync(module);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByIdAsync(module.Id);

            Assert.NotNull(result);
            Assert.Equal(module.Id, result.Id);
            Assert.Equal("Test Module", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingEntity_ReturnsNull()
        {
            var result = await _repo.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task IsExistAsync_ExistingEntity_ReturnsTrue()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            await _context.Modules.AddAsync(module);
            await _context.SaveChangesAsync();

            var result = await _repo.IsExistAsync(module.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task IsExistAsync_NonExistingEntity_ReturnsFalse()
        {
            var result = await _repo.IsExistAsync(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task IsExistAsync_DeletedEntity_ReturnsFalse()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10,
                IsDeleted = true
            };
            await _context.Modules.AddAsync(module);
            await _context.SaveChangesAsync();

            var result = await _repo.IsExistAsync(module.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task CountExistAsync_ReturnsCorrectCount()
        {
            var module1 = new Module { Id = Guid.NewGuid(), Name = "Module 1", Description = "Desc 1", CoreSlot = 1, TotalSlot = 2 };
            var module2 = new Module { Id = Guid.NewGuid(), Name = "Module 2", Description = "Desc 2", CoreSlot = 3, TotalSlot = 4 };
            var module3 = new Module { Id = Guid.NewGuid(), Name = "Module 3", Description = "Desc 3", CoreSlot = 5, TotalSlot = 6, IsDeleted = true };

            await _context.Modules.AddRangeAsync(module1, module2, module3);
            await _context.SaveChangesAsync();

            var ids = new List<Guid> { module1.Id, module2.Id, module3.Id, Guid.NewGuid() };
            var count = await _repo.CountExistAsync(ids);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPagedList()
        {
            var modules = Enumerable.Range(1, 25).Select(i => new Module
            {
                Id = Guid.NewGuid(),
                Name = $"Module {i}",
                Description = $"Description {i}",
                CoreSlot = i,
                TotalSlot = i + 1
            }).ToList();

            await _context.Modules.AddRangeAsync(modules);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Module>(new Dtos.QueryParamDto { Page = 1, Size = 10 });
            var result = await _repo.GetAllAsync(param);

            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal(25, result.TotalItemCount);
            Assert.Equal(3, result.PageCount);
        }

        [Fact]
        public async Task GetAllAsync_WithOrderBy_ReturnsOrderedPagedList()
        {
            var modules = new List<Module>
            {
                new() { Id = Guid.NewGuid(), Name = "B Module", Description = "Desc", CoreSlot = 1, TotalSlot = 2 },
                new() { Id = Guid.NewGuid(), Name = "A Module", Description = "Desc", CoreSlot = 1, TotalSlot = 2 },
                new() { Id = Guid.NewGuid(), Name = "C Module", Description = "Desc", CoreSlot = 1, TotalSlot = 2 }
            };

            await _context.Modules.AddRangeAsync(modules);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Module>(new Dtos.QueryParamDto { Page = 1, Size = 10, OrderBy = "Name", IsAsc = true });
            var result = await _repo.GetAllAsync(param);

            Assert.NotNull(result);
            Assert.Equal("A Module", result.First().Name);
        }

        [Fact]
        public async Task GetAllAsync_WithDescendingOrderBy_ReturnsDescendingOrderedPagedList()
        {
            var modules = new List<Module>
            {
                new() { Id = Guid.NewGuid(), Name = "A Module", Description = "Desc", CoreSlot = 1, TotalSlot = 2 },
                new() { Id = Guid.NewGuid(), Name = "B Module", Description = "Desc", CoreSlot = 1, TotalSlot = 2 },
                new() { Id = Guid.NewGuid(), Name = "C Module", Description = "Desc", CoreSlot = 1, TotalSlot = 2 }
            };

            await _context.Modules.AddRangeAsync(modules);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Module>(new Dtos.QueryParamDto { Page = 1, Size = 10, OrderBy = "Name", IsAsc = false });
            var result = await _repo.GetAllAsync(param);

            Assert.NotNull(result);
            Assert.Equal("C Module", result.First().Name);
        }

        [Fact]
        public void Update_UpdatesEntity()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Description = "Original Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            _context.Modules.Add(module);
            _context.SaveChanges();

            var originalUpdateTime = module.TimeUpdated;
            module.Name = "Updated Name";
            _repo.Update(module);
            _context.SaveChanges();

            var updated = _context.Modules.Find(module.Id);
            Assert.Equal("Updated Name", updated!.Name);
            Assert.True(updated.TimeUpdated > originalUpdateTime);
        }

        [Fact]
        public async Task SoftDeleteAsync_ExistingEntity_MarksAsDeleted()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            await _context.Modules.AddAsync(module);
            await _context.SaveChangesAsync();

            var result = await _repo.SoftDeleteAsync(module.Id);
            await _context.SaveChangesAsync();

            Assert.True(result);
            var deleted = await _context.Modules.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == module.Id);
            Assert.True(deleted!.IsDeleted);
            Assert.True(deleted.TimeUpdated > module.TimeCreated);
        }

        [Fact]
        public async Task SoftDeleteAsync_NonExistingEntity_ReturnsFalse()
        {
            var result = await _repo.SoftDeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task BulkSoftDeleteAsync_DeletesMultipleEntities()
        {
            var modules = new List<Module>
            {
                new() { Id = Guid.NewGuid(), Name = "Module 1", Description = "Desc 1", CoreSlot = 1, TotalSlot = 2, IsDeleted = false },
                new() { Id = Guid.NewGuid(), Name = "Module 2", Description = "Desc 2", CoreSlot = 3, TotalSlot = 4, IsDeleted = false },
                new() { Id = Guid.NewGuid(), Name = "Module 3", Description = "Desc 3", CoreSlot = 5, TotalSlot = 6, IsDeleted = false }
            };

            await _context.Modules.AddRangeAsync(modules);
            await _context.SaveChangesAsync();

            var idsToDelete = new List<Guid> { modules[0].Id, modules[1].Id };
            var count = await _repo.BulkSoftDeleteAsync(m => idsToDelete.Contains(m.Id));

            Assert.Equal(2, count);

            var remaining = await _context.Modules.IgnoreQueryFilters().CountAsync(m => m.IsDeleted);
            Assert.Equal(2, remaining);
        }

        [Fact]
        public async Task CountAsync_ReturnsTotalCount()
        {
            var modules = new List<Module>
            {
                new() { Id = Guid.NewGuid(), Name = "Module 1", Description = "Desc 1", CoreSlot = 1, TotalSlot = 2 },
                new() { Id = Guid.NewGuid(), Name = "Module 2", Description = "Desc 2", CoreSlot = 3, TotalSlot = 4 }
            };

            await _context.Modules.AddRangeAsync(modules);
            await _context.SaveChangesAsync();

            var count = await _repo.CountAsync();

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task CountAsync_WithDeletedEntities_ReturnsCount()
        {
            var modules = new List<Module>
            {
                new() { Id = Guid.NewGuid(), Name = "Module 1", Description = "Desc 1", CoreSlot = 1, TotalSlot = 2 },
                new() { Id = Guid.NewGuid(), Name = "Module 2", Description = "Desc 2", CoreSlot = 3, TotalSlot = 4, IsDeleted = true }
            };

            await _context.Modules.AddRangeAsync(modules);
            await _context.SaveChangesAsync();

            var count = await _repo.CountAsync();

            Assert.Equal(1, count);
        }
    }
}
