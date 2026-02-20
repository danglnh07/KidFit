using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using KidFit.Validators;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KidFit.Tests.Services
{
    public class CardServiceTests : IDisposable
    {
        private readonly CardService _service;
        private readonly SqliteConnection _conn;
        private readonly AppDbContext _context;

        public CardServiceTests()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_conn)
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            var uow = new UnitOfWork(_context);
            var cardValidator = new CardValidator();
            _service = new CardService(uow, cardValidator);
        }

        public void Dispose()
        {
            _context.Dispose();
            _conn.Close();
            _conn.Dispose();
        }

        [Fact]
        public async Task CreateCard_CreatesCard()
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

            var card = new Card()
            {
                Id = Guid.NewGuid(),
                Name = "Test Card",
                Description = "Test Description",
                Image = "test.jpg",
                CategoryId = category.Id
            };

            var result = await _service.CreateCard(card);

            Assert.True(result);

            var dbEntity = _context.Cards.FirstOrDefault(c => c.Id == card.Id);
            Assert.NotNull(dbEntity);
            Assert.Equal(card.Name, dbEntity.Name);
            Assert.Equal(card.Description, dbEntity.Description);
            Assert.Equal(card.Image, dbEntity.Image);
        }

        [Fact]
        public async Task CreateCard_InvalidData_ThrowsValidationException()
        {
            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            _context.CardCategories.Add(category);
            await _context.SaveChangesAsync();

            var card = new Card()
            {
                Id = Guid.NewGuid(),
                Name = "",
                Description = "",
                Image = "",
                CategoryId = category.Id
            };

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateCard(card));
        }

        [Fact]
        public async Task CreateCard_CategoryNotFound_ThrowsDependentEntityNotFoundException()
        {
            var card = new Card()
            {
                Id = Guid.NewGuid(),
                Name = "Test Card",
                Description = "Test Description",
                Image = "test.jpg",
                CategoryId = Guid.NewGuid()
            };

            await Assert.ThrowsAsync<DependentEntityNotFoundException>(
                () => _service.CreateCard(card));
        }

        [Fact]
        public async Task GetCard_ReturnsCard()
        {
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
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            var result = await _service.GetCard(card.Id);

            Assert.NotNull(result);
            Assert.Equal(card.Name, result.Name);
            Assert.Equal(card.Description, result.Description);
            Assert.Equal(card.Image, result.Image);
            Assert.Equal(card.CategoryId, result.CategoryId);
        }

        [Fact]
        public async Task GetCard_WithNestedData_ReturnsCardWithCategory()
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
                Description = "Test Description",
                Image = "test.jpg",
                CategoryId = category.Id
            };
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            var result = await _service.GetCard(card.Id, true);

            Assert.NotNull(result);
            Assert.Equal(card.Name, result.Name);
            Assert.Equal(card.Description, result.Description);
            Assert.Equal(card.Image, result.Image);
            Assert.Equal(card.CategoryId, result.CategoryId);
            Assert.NotNull(result.Category);
            Assert.Equal(category.Name, result.Category.Name);
            Assert.Equal(category.Description, result.Category.Description);
            Assert.Equal(category.BorderColor, result.Category.BorderColor);
        }

        [Fact]
        public async Task GetCard_NotFound_ReturnsNull()
        {
            var result = await _service.GetCard(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllCards_ReturnsPagedList()
        {
            var category = new CardCategory()
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };

            var card1 = new Card()
            {
                Id = Guid.NewGuid(),
                Name = "Card 1",
                Description = "Description 1",
                Image = "test1.jpg",
                CategoryId = category.Id
            };
            var card2 = new Card()
            {
                Id = Guid.NewGuid(),
                Name = "Card 2",
                Description = "Description 2",
                Image = "test2.jpg",
                CategoryId = category.Id
            };
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddRangeAsync(card1, card2);
            await _context.SaveChangesAsync();

            var result = await _service.GetAllCards(new QueryParam<Card>(page: 1, size: 10));

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateCard_UpdatesCard()
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
                Name = "Original Name",
                Description = "Original Description",
                Image = "original.jpg",
                CategoryId = category.Id
            };
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            card.Name = "Updated Name";
            card.Description = "Updated Description";

            var result = await _service.UpdateCard(card);

            Assert.True(result);

            var dbEntity = await _context.Cards.FirstOrDefaultAsync(c => c.Id == card.Id);
            Assert.NotNull(dbEntity);
            Assert.Equal(card.Name, dbEntity.Name);
            Assert.Equal(card.Description, dbEntity.Description);
            Assert.Equal(card.Image, dbEntity.Image);
            Assert.Equal(card.CategoryId, dbEntity.CategoryId);
        }

        [Fact]
        public async Task UpdateCard_InvalidData_ThrowsValidationException()
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
                Name = "Valid Name",
                Description = "Valid Description",
                Image = "valid.jpg",
                CategoryId = category.Id
            };
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            card.Name = "";
            card.Description = "";

            await Assert.ThrowsAsync<ValidationException>(
                () => _service.UpdateCard(card));
        }

        [Fact]
        public async Task UpdateCard_CategoryNotFound_ThrowsDependentEntityNotFoundException()
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
                Name = "Valid Name",
                Description = "Valid Description",
                Image = "valid.jpg",
                CategoryId = category.Id
            };
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            card.CategoryId = Guid.NewGuid();

            await Assert.ThrowsAsync<DependentEntityNotFoundException>(
                () => _service.UpdateCard(card));
        }

        [Fact]
        public async Task DeleteCard_DeletesCard()
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
                Description = "Test Description",
                Image = "test.jpg",
                CategoryId = category.Id
            };
            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteCard(card.Id);

            Assert.True(result);

            var deletedCard = _context.Cards.FirstOrDefault(c => c.Id == card.Id);
            Assert.Null(deletedCard);
        }

        [Fact]
        public async Task DeleteCard_NotFound_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteCard(Guid.NewGuid()));
        }

    }
}
