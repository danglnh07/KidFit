using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using KidFit.Validators;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace KidFit.Tests.Services
{
    public class CardCategoryServiceTests : IDisposable
    {
        private readonly CardCategoryService _service;
        private readonly SqliteConnection _conn;
        private readonly AppDbContext _context;

        public CardCategoryServiceTests()
        {
            // Create SQLite connection
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();

            // Create db context
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_conn)
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            // Create dependencies for service
            var uow = new UnitOfWork(_context);
            var cardCategoryValidator = new CardCategoryValidator();
            var logger = new Mock<ILogger<CardCategoryService>>().Object;
            _service = new CardCategoryService(uow, cardCategoryValidator, logger);
        }

        public void Dispose()
        {
            _context.Dispose();
            _conn.Close();
            _conn.Dispose();
        }

        [Fact]
        public async Task CreateCardCategory_CreatesCategory()
        {
            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };

            var result = await _service.CreateCardCategory(category);

            Assert.True(result);

            // Get the same entity from database to check if it truly get added
            var dbEntity = _context.CardCategories.FirstOrDefault(c => c.Id == category.Id);
            Assert.NotNull(dbEntity);
            Assert.Equal(category.Name, dbEntity.Name);
            Assert.Equal(category.Description, dbEntity.Description);
            Assert.Equal(category.BorderColor, dbEntity.BorderColor);
        }

        [Fact]
        public async Task CreateCardCategory_InvalidData_ThrowsValidationException()
        {
            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "",
                Description = " ",
                BorderColor = " "
            };

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateCardCategory(category));
        }

        [Fact]
        public async Task GetCardCategory_ReturnsCategory()
        {
            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            await _context.CardCategories.AddAsync(category);
            await _context.SaveChangesAsync();

            var result = await _service.GetCardCategory(category.Id);

            Assert.NotNull(result);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Description, result.Description);
            Assert.Equal(category.BorderColor, result.BorderColor);
        }

        [Fact]
        public async Task GetCardCategory_NotFound_ReturnsNull()
        {
            var result = await _service.GetCardCategory(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllCardCategories_ReturnsPagedList()
        {
            var category1 = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Category 1",
                Description = "Description 1",
                BorderColor = "#FF0000"
            };
            var category2 = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Category 2",
                Description = "Description 2",
                BorderColor = "#00FF00"
            };
            await _context.CardCategories.AddRangeAsync(category1, category2);
            await _context.SaveChangesAsync();

            var result = await _service.GetAllCardCategories(new QueryParam<CardCategory>(page: 1, size: 10));

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateCardCategory_UpdatesCategory()
        {
            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Description = "Original Description",
                BorderColor = "#FF0000"
            };
            await _context.CardCategories.AddAsync(category);
            await _context.SaveChangesAsync();

            category.Name = "Updated Name";
            category.Description = "Updated Description";

            var result = await _service.UpdateCardCategory(category);

            Assert.True(result);

            var dbEntity = await _context.CardCategories.FirstOrDefaultAsync(c => c.Id == category.Id);
            Assert.NotNull(dbEntity);
            Assert.Equal(category.Name, dbEntity.Name);
            Assert.Equal(category.Description, dbEntity.Description);
        }

        [Fact]
        public async Task UpdateCardCategory_InvalidData_ThrowsValidationException()
        {
            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = "Valid Description",
                BorderColor = "#FF0000"
            };

            await _context.CardCategories.AddAsync(category);
            await _context.SaveChangesAsync();

            category.Name = "";
            category.Description = "";

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.UpdateCardCategory(category));
        }

        [Fact]
        public async Task DeleteCardCategory_DeletesCategoryAndCascadeCards()
        {
            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };

            var card = new Card()
            {
                Id = Guid.NewGuid(),
                Name = "Test Card",
                Description = "Test Card Description",
                Image = "test.jpg",
                CategoryId = category.Id
            };
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteCardCategory(category.Id);

            Assert.True(result);

            var deletedCategory = await _context.CardCategories.FirstOrDefaultAsync(c => c.Id == category.Id);
            Assert.Null(deletedCategory);

            var deletedCard = await _context.Cards.FirstOrDefaultAsync(c => c.Id == card.Id);
            Assert.Null(deletedCard);
        }

        [Fact]
        public async Task DeleteCardCategory_NotFound_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteCardCategory(Guid.NewGuid()));
        }


    }
}
