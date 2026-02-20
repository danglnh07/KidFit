using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using KidFit.Validators;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace KidFit.Tests.Services
{
    public class ModuleServiceTests : IDisposable
    {
        private readonly ModuleService _service;
        private readonly SqliteConnection _conn;
        private readonly AppDbContext _context;

        public ModuleServiceTests()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_conn)
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            var uow = new UnitOfWork(_context);
            var moduleValidator = new ModuleValidator();
            var logger = new Mock<ILogger<ModuleService>>().Object;
            _service = new ModuleService(uow, moduleValidator, logger);
        }

        public void Dispose()
        {
            _context.Dispose();
            _conn.Close();
            _conn.Dispose();
        }

        [Fact]
        public async Task CreateModule_CreatesModule()
        {
            var module = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            var result = await _service.CreateModule(module);

            Assert.True(result);

            var dbEntity = _context.Modules.FirstOrDefault(m => m.Id == module.Id);
            Assert.NotNull(dbEntity);
            Assert.Equal(module.Name, dbEntity.Name);
            Assert.Equal(module.Description, dbEntity.Description);
            Assert.Equal(module.CoreSlot, dbEntity.CoreSlot);
            Assert.Equal(module.TotalSlot, dbEntity.TotalSlot);
        }

        [Fact]
        public async Task CreateModule_InvalidData_ThrowsValidationException()
        {
            var module = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "",
                Description = "",
                CoreSlot = 0,
                TotalSlot = 0
            };

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateModule(module));
        }

        [Fact]
        public async Task CreateModule_CoreSlotGreaterThanTotalSlot_ThrowsValidationException()
        {
            var module = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 10,
                TotalSlot = 5
            };

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateModule(module));
        }

        [Fact]
        public async Task GetModule_ReturnsModule()
        {
            var module = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            await _context.Modules.AddAsync(module);
            await _context.SaveChangesAsync();

            var result = await _service.GetModule(module.Id);

            Assert.NotNull(result);
            Assert.Equal(module.Name, result.Name);
            Assert.Equal(module.Description, result.Description);
            Assert.Equal(module.CoreSlot, result.CoreSlot);
            Assert.Equal(module.TotalSlot, result.TotalSlot);
        }

        [Fact]
        public async Task GetModule_NotFound_ReturnsNull()
        {
            var result = await _service.GetModule(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetModules_ReturnsPagedList()
        {
            var module1 = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Module 1",
                Description = "Description 1",
                CoreSlot = 5,
                TotalSlot = 10
            };
            var module2 = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Module 2",
                Description = "Description 2",
                CoreSlot = 3,
                TotalSlot = 8
            };

            await _context.Modules.AddRangeAsync(module1, module2);
            await _context.SaveChangesAsync();

            var result = await _service.GetModules(new QueryParam<Module>(page: 1, size: 10));

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateModule_UpdatesModule()
        {
            var module = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Description = "Original Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            await _context.Modules.AddAsync(module);
            await _context.SaveChangesAsync();

            module.Name = "Updated Name";
            module.Description = "Updated Description";

            var result = await _service.UpdateModule(module);

            Assert.True(result);

            var dbEntity = await _context.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
            Assert.NotNull(dbEntity);
            Assert.Equal(module.Name, dbEntity.Name);
            Assert.Equal(module.Description, dbEntity.Description);
            Assert.Equal(module.CoreSlot, dbEntity.CoreSlot);
            Assert.Equal(module.TotalSlot, dbEntity.TotalSlot);
        }

        [Fact]
        public async Task UpdateModule_InvalidData_ThrowsValidationException()
        {
            var module = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = "Valid Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            await _context.Modules.AddAsync(module);
            await _context.SaveChangesAsync();

            module.Name = "";
            module.Description = "";

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.UpdateModule(module));
        }

        [Fact]
        public async Task DeleteModule_DeletesModuleAndCascadeLessons()
        {
            var module = new Module()
            {
                Id = Guid.NewGuid(),
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            var lesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = module.Id,
                Year = Year.SEED,
                CardIds = []
            };
            await _context.Modules.AddAsync(module);
            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteModule(module.Id);

            Assert.True(result);

            var deletedModule = await _service.GetModule(module.Id);
            Assert.Null(deletedModule);

            var deletedLesson = _context.Lessons.FirstOrDefault(l => l.Id == lesson.Id);
            Assert.Null(deletedLesson);
        }

        [Fact]
        public async Task DeleteModule_NotFound_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteModule(Guid.NewGuid()));
        }
    }
}
