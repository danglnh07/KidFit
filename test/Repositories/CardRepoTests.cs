using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KidFit.Tests.Repositories
{
    [Collection("RepositoryTests")]
    public class CardRepoTests : IClassFixture<PostgresFixture>, IAsyncLifetime
    {
        private readonly AppDbContext _context;
        private readonly CardRepo _repo;
        private IDbContextTransaction _transaction = null!;

        public CardRepoTests(PostgresFixture fixture)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(fixture.ConnectionString).Options;
            _context = new AppDbContext(options);
            _repo = new CardRepo(_context);
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
        public async Task GetByIdWithNestedDataAsync_ExistingEntity_ReturnsEntityWithCategory()
        {
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

            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();

            var result = await _repo.GetByIdWithNestedDataAsync(card.Id);

            Assert.NotNull(result);
            Assert.Equal(card.Id, result.Id);
            Assert.NotNull(result.Category);
            Assert.Equal(category.Name, result.Category.Name);
        }

        [Fact]
        public async Task GetByIdWithNestedDataAsync_NonExistingEntity_ReturnsNull()
        {
            var result = await _repo.GetByIdWithNestedDataAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllWithNestedDataAsync_ReturnsPagedListWithCategories()
        {
            var category = new CardCategory
            {
                Id = Guid.NewGuid(),
                Name = $"Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            var cards = Enumerable.Range(1, 25).Select(i => new Card
            {
                Id = Guid.NewGuid(),
                Name = $"Card {i}",
                Description = $"Description {i}",
                Image = $"image{i}.png",
                Category = category
            }).ToList();

            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddRangeAsync(cards);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Card>(page: 1, size: 10);
            var result = await _repo.GetAllWithNestedDataAsync(param);

            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal(25, result.TotalItemCount);
            Assert.All(result, card => Assert.NotNull(card.Category));
        }

        [Fact]
        public async Task GetAllWithNestedDataAsync_WithOrderBy_ReturnsOrderedPagedList()
        {
            var category = new CardCategory
            {
                Id = Guid.NewGuid(),
                Name = $"Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            var cards = new List<Card>
            {
                new() { Id = Guid.NewGuid(), Name = $"B Card", Description = "Desc", Image = "b.png", Category = category },
                new() { Id = Guid.NewGuid(), Name = $"A Card", Description = "Desc", Image = "a.png", Category = category },
                new() { Id = Guid.NewGuid(), Name = $"C Card", Description = "Desc", Image = "c.png", Category = category }
            };

            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddRangeAsync(cards);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Card>(page: 1, size: 10, orderBy: "Name", isAsc: true);
            var result = await _repo.GetAllWithNestedDataAsync(param);

            Assert.NotNull(result);
            Assert.Equal(cards[1].Name, result.First().Name);
            Assert.NotNull(result.First().Category);
            Assert.Equal(category.Name, result.First().Category.Name);
        }

        [Fact]
        public async Task GetAllWithNestedDataAsync_WithDescendingOrderBy_ReturnsDescendingOrderedPagedList()
        {
            var category = new CardCategory
            {
                Id = Guid.NewGuid(),
                Name = $"Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            var cards = new List<Card>
            {
                new() { Id = Guid.NewGuid(), Name = $"A Card", Description = "Desc", Image = "a.png", Category = category },
                new() { Id = Guid.NewGuid(), Name = $"B Card", Description = "Desc", Image = "b.png", Category = category },
                new() { Id = Guid.NewGuid(), Name = $"C Card", Description = "Desc", Image = "c.png", Category = category }
            };

            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddRangeAsync(cards);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Card>(page: 1, size: 10, orderBy: "Name", isAsc: false);
            var result = await _repo.GetAllWithNestedDataAsync(param);

            Assert.NotNull(result);
            Assert.Equal(cards[2].Name, result.First().Name);
            Assert.NotNull(result.First().Category);
            Assert.Equal(category.Name, result.First().Category.Name);
        }

        [Fact]
        public async Task GetAllWithNestedDataAsync_SecondPage_ReturnsCorrectPage()
        {
            var category = new CardCategory
            {
                Id = Guid.NewGuid(),
                Name = $"Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };
            var cards = Enumerable.Range(1, 25).Select(i => new Card
            {
                Id = Guid.NewGuid(),
                Name = $"Card {i}",
                Description = $"Description {i}",
                Image = $"image{i}.png",
                Category = category
            }).ToList();

            await _context.CardCategories.AddAsync(category);
            await _context.Cards.AddRangeAsync(cards);
            await _context.SaveChangesAsync();

            var param = new QueryParam<Card>(page: 2, size: 10);
            var result = await _repo.GetAllWithNestedDataAsync(param);

            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal(2, result.PageNumber);
        }

    }
}
