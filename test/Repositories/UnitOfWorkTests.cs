using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KidFit.Tests.Repositories
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            // Use SQLite in-memory database for consistency with GenericRepoTests
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
            _unitOfWork = new UnitOfWork(_context);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        [Fact]
        public void Repo_SameType_ReturnsSameInstance()
        {
            var repo1 = _unitOfWork.Repo<Module>();
            var repo2 = _unitOfWork.Repo<Module>();

            Assert.Same(repo1, repo2);
        }

        [Fact]
        public void Repo_DifferentTypes_ReturnsDifferentInstances()
        {
            var moduleRepo = _unitOfWork.Repo<Module>();
            var cardRepo = _unitOfWork.Repo<Card>();

            Assert.NotSame(moduleRepo, cardRepo);
        }

        [Fact]
        public void Repo_ReturnsIGenericRepo()
        {
            var repo = _unitOfWork.Repo<Module>();

            Assert.IsAssignableFrom<IGenericRepo<Module>>(repo);
        }

        [Fact]
        public async Task SaveChangesAsync_PersistsChangesToDatabase()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            var repo = _unitOfWork.Repo<Module>();
            await repo.CreateAsync(module);
            var changes = await _unitOfWork.SaveChangesAsync();

            Assert.Equal(1, changes);
            var saved = await _context.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
            Assert.NotNull(saved);
            Assert.Equal("Test Module", saved.Name);
        }

        [Fact]
        public async Task SaveChangesAsync_NoChanges_ReturnsZero()
        {
            var changes = await _unitOfWork.SaveChangesAsync();

            Assert.Equal(0, changes);
        }

        [Fact]
        public async Task SaveChangesAsync_MultipleChanges_SavesAll()
        {
            var modules = new List<Module>
            {
                new() { Id = Guid.NewGuid(), Name = "Module 1", Description = "Desc 1", CoreSlot = 1, TotalSlot = 2 },
                new() { Id = Guid.NewGuid(), Name = "Module 2", Description = "Desc 2", CoreSlot = 3, TotalSlot = 4 }
            };

            var repo = _unitOfWork.Repo<Module>();
            await repo.CreateBatchAsync(modules);
            var changes = await _unitOfWork.SaveChangesAsync();

            Assert.Equal(2, changes);
            var count = await _context.Modules.CountAsync();
            Assert.Equal(2, count);
        }

        [Fact]
        public void Dispose_DisposesContext()
        {
            _unitOfWork.Dispose();

            // After disposal, context should not be usable
            Assert.Throws<ObjectDisposedException>(() => _context.Modules.ToList());
        }

        [Fact]
        public async Task MultipleOperations_WithinSameUnitOfWork_AreCommittedTogether()
        {
            var moduleRepo = _unitOfWork.Repo<Module>();
            var cardCategoryRepo = _unitOfWork.Repo<CardCategory>();

            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            var category = new CardCategory
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };

            await moduleRepo.CreateAsync(module);
            await cardCategoryRepo.CreateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var savedModule = await _context.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
            var savedCategory = await _context.CardCategories.FirstOrDefaultAsync(c => c.Id == category.Id);

            Assert.NotNull(savedModule);
            Assert.NotNull(savedCategory);
        }

        [Fact]
        public async Task Repositories_ShareSameContext()
        {
            var moduleRepo = _unitOfWork.Repo<Module>();
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            await moduleRepo.CreateAsync(module);
            await _unitOfWork.SaveChangesAsync();

            // Get a new repo instance from the same unit of work
            var anotherModuleRepo = _unitOfWork.Repo<Module>();
            var found = await anotherModuleRepo.IsExistAsync(module.Id);

            Assert.True(found);
        }
    }
}
