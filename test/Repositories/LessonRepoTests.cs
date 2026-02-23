using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Constants;
using KidFit.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KidFit.Tests.Repositories
{
    [Collection("RepositoryTests")]
    public class LessonRepoTests : IClassFixture<PostgresFixture>, IAsyncLifetime
    {
        private readonly AppDbContext _context;
        private readonly LessonRepo _repo;
        private IDbContextTransaction _transaction = null!;

        public LessonRepoTests(PostgresFixture fixture)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(fixture.ConnectionString).Options;
            _context = new AppDbContext(options);
            _repo = new LessonRepo(_context);
        }

        public async Task InitializeAsync()
        {
            // Start each test in isolation transaction
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task DisposeAsync()
        {
            // Rollback transaction after complete a test to avoid sharing data between tests
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        [Fact]
        public async Task GetByIdWithNestedDataAsync_ExistingEntity_ReturnsEntityWithModuleAndCards()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = $"Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            var category = new CardCategory
            {
                Id = Guid.NewGuid(),
                Name = $"Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            var card = new Card
            {
                Id = Guid.NewGuid(),
                Name = $"Test Card",
                Description = "Test Description",
                Image = "test.png",
                Category = category
            };
            var lesson = new Lesson
            {
                Id = Guid.NewGuid(),
                Name = $"Test Lesson",
                Content = "Test Content",
                Module = module,
                Year = Year.SEED,
                Cards = [card]
            };

            await _context.Modules.AddAsync(module);
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByIdWithNestedDataAsync(lesson.Id);

            Assert.NotNull(result);
            Assert.Equal(lesson.Id, result.Id);
            Assert.NotNull(result.Module);
            Assert.Equal(module.Name, result.Module.Name);
            Assert.NotEmpty(result.Cards);
        }

        [Fact]
        public async Task GetByIdWithNestedDataAsync_NonExistingEntity_ReturnsNull()
        {
            var result = await _repo.GetByIdWithNestedDataAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllWithNestedDataAsync_ReturnsPagedListWithModulesAndCards()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = $"Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            var category = new CardCategory
            {
                Id = Guid.NewGuid(),
                Name = $"Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            var card = new Card
            {
                Id = Guid.NewGuid(),
                Name = $"Test Card",
                Description = "Test Description",
                Image = "test.png",
                Category = category
            };
            var lessons = Enumerable.Range(1, 25).Select(i => new Lesson
            {
                Id = Guid.NewGuid(),
                Name = $"Lesson {i}",
                Content = $"Content {i}",
                Module = module,
                Year = Year.SEED,
                Cards = [card]
            }).ToList();

            await _context.Modules.AddAsync(module);
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.Lessons.AddRangeAsync(lessons);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Lesson>(page: 1, size: 10);
            var result = await _repo.GetAllWithNestedDataAsync(param);

            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal(25, result.TotalItemCount);
            Assert.All(result, lesson => Assert.NotNull(lesson.Module));
        }

        [Fact]
        public async Task GetAllWithNestedDataAsync_WithOrderBy_ReturnsOrderedPagedList()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = $"Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            var lessons = new List<Lesson>
            {
                new() { Id = Guid.NewGuid(), Name = $"B Lesson", Content = "Content", Module = module, Year = Year.SEED },
                new() { Id = Guid.NewGuid(), Name = $"A Lesson", Content = "Content", Module = module, Year = Year.SEED },
                new() { Id = Guid.NewGuid(), Name = $"C Lesson", Content = "Content", Module = module, Year = Year.SEED }
            };

            await _context.Modules.AddAsync(module);
            await _context.Lessons.AddRangeAsync(lessons);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Lesson>(page: 1, size: 10, orderBy: "Name", isAsc: true);
            var result = await _repo.GetAllWithNestedDataAsync(param);

            Assert.NotNull(result);
            Assert.Equal($"A Lesson", result.First().Name);
        }

        [Fact]
        public async Task GetAllWithNestedDataAsync_WithDescendingOrderBy_ReturnsDescendingOrderedPagedList()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = $"Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            var lessons = new List<Lesson>
            {
                new() { Id = Guid.NewGuid(), Name = $"A Lesson", Content = "Content", Module = module, Year = Year.SEED },
                new() { Id = Guid.NewGuid(), Name = $"B Lesson", Content = "Content", Module = module, Year = Year.SEED },
                new() { Id = Guid.NewGuid(), Name = $"C Lesson", Content = "Content", Module = module, Year = Year.SEED }
            };

            await _context.Modules.AddAsync(module);
            await _context.Lessons.AddRangeAsync(lessons);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Lesson>(page: 1, size: 10, orderBy: "Name", isAsc: false);
            var result = await _repo.GetAllWithNestedDataAsync(param);

            Assert.NotNull(result);
            Assert.Equal($"C Lesson", result.First().Name);
        }

        [Fact]
        public async Task GetAllWithNestedDataAsync_SecondPage_ReturnsCorrectPage()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = $"Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            var lessons = Enumerable.Range(1, 25).Select(i => new Lesson
            {
                Id = Guid.NewGuid(),
                Name = $"Lesson {i} Page",
                Content = $"Content {i}",
                Module = module,
                Year = Year.SEED
            }).ToList();

            await _context.Modules.AddAsync(module);
            await _context.Lessons.AddRangeAsync(lessons);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Lesson>(page: 2, size: 10);
            var result = await _repo.GetAllWithNestedDataAsync(param);

            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal(2, result.PageNumber);
        }

        [Fact]
        public async Task GetByIdWithNestedDataAsync_WithoutCards_ReturnsEntityWithEmptyCardsList()
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                Name = $"Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };
            var lesson = new Lesson
            {
                Id = Guid.NewGuid(),
                Name = $"Test Lesson",
                Content = "Test Content",
                Module = module,
                Year = Year.SEED,
                Cards = []
            };

            await _context.Modules.AddAsync(module);
            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByIdWithNestedDataAsync(lesson.Id);

            Assert.NotNull(result);
            Assert.NotNull(result.Module);
            Assert.Empty(result.Cards);
        }
    }
}
