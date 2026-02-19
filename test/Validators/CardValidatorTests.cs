using KidFit.Models;
using KidFit.Validators;

namespace KidFit.Tests.Validators
{
    public class CardValidatorTests
    {
        private readonly CardValidator _validator;

        public CardValidatorTests()
        {
            _validator = new CardValidator();
        }

        [Fact]
        public void Validate_ValidCard_ReturnsSuccess()
        {
            var card = new Card
            {
                Name = "Test Card",
                Description = "Test Description",
                Image = "image.jpg",
                CategoryId = Guid.NewGuid()
            };

            var result = _validator.Validate(card);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyName_ReturnsFailure(string? name)
        {
            var card = new Card
            {
                Name = name!,
                Description = "Test Description",
                Image = "image.jpg",
                CategoryId = Guid.NewGuid()
            };

            var result = _validator.Validate(card);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyDescription_ReturnsFailure(string? description)
        {
            var card = new Card
            {
                Name = "Test Card",
                Description = description!,
                Image = "image.jpg",
                CategoryId = Guid.NewGuid()
            };

            var result = _validator.Validate(card);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Description");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyImage_ReturnsFailure(string? image)
        {
            var card = new Card
            {
                Name = "Test Card",
                Description = "Test Description",
                Image = image!,
                CategoryId = Guid.NewGuid()
            };

            var result = _validator.Validate(card);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Image");
        }

        [Fact]
        public void Validate_EmptyCategoryId_ReturnsFailure()
        {
            var card = new Card
            {
                Name = "Test Card",
                Description = "Test Description",
                Image = "image.jpg",
                CategoryId = Guid.Empty
            };

            var result = _validator.Validate(card);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CategoryId");
        }

        [Fact]
        public void Validate_MultipleErrors_ReturnsAllFailures()
        {
            var card = new Card
            {
                Name = "",
                Description = "",
                Image = "",
                CategoryId = Guid.Empty
            };

            var result = _validator.Validate(card);

            Assert.False(result.IsValid);
            Assert.Equal(4, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
            Assert.Contains(result.Errors, e => e.PropertyName == "Description");
            Assert.Contains(result.Errors, e => e.PropertyName == "Image");
            Assert.Contains(result.Errors, e => e.PropertyName == "CategoryId");
        }
    }
}
