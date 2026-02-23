using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using KidFit.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KidFit.Tests.Services
{
    public class LessonServiceTests : IClassFixture<PostgresFixture>, IAsyncLifetime
    {
        private readonly LessonService _service;
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction = null!;

        public LessonServiceTests(PostgresFixture fixture)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(fixture.ConnectionString).Options;
            _context = new AppDbContext(options);

            var uow = new UnitOfWork(_context);
            var lessonValidator = new LessonValidator();
            _service = new LessonService(uow, lessonValidator);
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

        private async Task<(Module module, Card card)> SeedTestDataAsync()
        {
            var module = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            _context.CardCategories.Add(category);

            var card = new Card()
            {
                Id = Guid.NewGuid(),
                Name = "Test Card",
                Description = "Test Description",
                Image = "test.jpg",
                CategoryId = category.Id
            };
            await _context.Modules.AddAsync(module);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            return (module, card);
        }

        [Fact]
        public async Task CreateLessonAsync_CreatesLesson()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            var result = await _service.CreateLessonAsync(lesson);

            Assert.True(result);

            var dbEntity = _context.Lessons.FirstOrDefault(l => l.Id == lesson.Id);
            Assert.NotNull(dbEntity);
            Assert.Equal(lesson.Name, dbEntity.Name);
            Assert.Equal(lesson.Content, dbEntity.Content);
        }

        [Fact]
        public async Task CreateLessonAsync_InvalidData_ThrowsValidationException()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "",
                Content = "",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateLessonAsync(lesson));
        }

        [Fact]
        public async Task CreateLessonAsync_ModuleNotFound_ThrowsDependentEntityNotFoundException()
        {
            var (_, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = Guid.NewGuid(),
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await Assert.ThrowsAsync<DependentEntityNotFoundException>(() => _service.CreateLessonAsync(lesson));
        }

        [Fact]
        public async Task CreateLessonAsync_CardNotFound_ThrowsDependentEntityNotFoundException()
        {
            var (module, _) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [Guid.NewGuid()]
            };

            await Assert.ThrowsAsync<DependentEntityNotFoundException>(() => _service.CreateLessonAsync(lesson));
        }

        [Fact]
        public async Task GetLessonAsync_ReturnsLesson()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            var result = await _service.GetLessonAsync(lesson.Id);

            Assert.NotNull(result);
            Assert.Equal(lesson.Name, result.Name);
            Assert.Equal(lesson.Content, result.Content);
            Assert.Equal(lesson.ModuleId, result.ModuleId);
            Assert.Equal(lesson.Year, result.Year);
            Assert.Equal(lesson.CardIds, result.CardIds);
        }

        [Fact]
        public async Task GetLessonAsync_WithNestedData_ReturnsLessonWithModuleAndCards()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            var result = await _service.GetLessonAsync(lesson.Id, true);

            Assert.NotNull(result);
            Assert.Equal(lesson.Name, result.Name);
            Assert.Equal(lesson.Content, result.Content);
            Assert.Equal(lesson.Year, result.Year);
            Assert.NotNull(result.Module);
            Assert.Equal(module.Name, result.Module.Name);
        }

        [Fact]
        public async Task GetLessonAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetLessonAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllLessonsAsync_ReturnsPagedList()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson1 = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Lesson 1",
                Content = "Content 1",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };
            var lesson2 = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Lesson 2",
                Content = "Content 2",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await _context.Lessons.AddRangeAsync(lesson1, lesson2);
            await _context.SaveChangesAsync();

            var result = await _service.GetAllLessonsAsync(new QueryParam<Lesson>(page: 1, size: 10));

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateLessonAsync_UpdatesLesson()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Content = "Original Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            lesson.Name = "Updated Name";
            lesson.Content = "Updated Content";

            var result = await _service.UpdateLessonAsync(lesson);

            Assert.True(result);

            var dbEntity = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lesson.Id);
            Assert.NotNull(dbEntity);
            Assert.Equal(lesson.Name, dbEntity.Name);
            Assert.Equal(lesson.Content, dbEntity.Content);
            Assert.Equal(lesson.ModuleId, dbEntity.ModuleId);
            Assert.Equal(lesson.Year, dbEntity.Year);
            Assert.Equal(lesson.CardIds, dbEntity.CardIds);
        }

        [Fact]
        public async Task UpdateLessonAsync_InvalidData_ThrowsValidationException()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Content = "Valid Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            lesson.Name = "";
            lesson.Content = "";

            await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateLessonAsync(lesson));
        }

        [Fact]
        public async Task UpdateLessonAsync_ModuleNotFound_ThrowsDependentEntityNotFoundException()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Content = "Valid Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            lesson.ModuleId = Guid.NewGuid();

            await Assert.ThrowsAsync<DependentEntityNotFoundException>(() => _service.UpdateLessonAsync(lesson));
        }

        [Fact]
        public async Task UpdateLessonAsync_CardNotFound_ThrowsDependentEntityNotFoundException()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Content = "Valid Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            lesson.CardIds = [Guid.NewGuid()];

            await Assert.ThrowsAsync<DependentEntityNotFoundException>(() => _service.UpdateLessonAsync(lesson));
        }

        [Fact]
        public async Task DeleteLessonAsync_DeletesLesson()
        {
            var (module, card) = await SeedTestDataAsync();

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = [card.Id]
            };

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteLessonAsync(lesson.Id);

            Assert.True(result);

            var deletedLesson = _context.Lessons.FirstOrDefault(l => l.Id == lesson.Id);
            Assert.Null(deletedLesson);
        }

        [Fact]
        public async Task DeleteLessonAsync_NotFound_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteLessonAsync(Guid.NewGuid()));
        }
    }
}
