using KidFit.Models;
using KidFit.Validators;

namespace KidFit.Tests.Validators
{
    public class CardCategoryValidatorTests
    {
        private readonly CardCategoryValidator _validator;

        public CardCategoryValidatorTests()
        {
            _validator = new CardCategoryValidator();
        }

        [Fact]
        public void Validate_ValidCardCategory_ReturnsSuccess()
        {
            var cardCategory = new CardCategory
            {
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = "#FF0000"
            };

            var result = _validator.Validate(cardCategory);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyName_ReturnsFailure(string? name)
        {
            var cardCategory = new CardCategory
            {
                Name = name!,
                Description = "Test Description",
                BorderColor = "#FF0000"
            };

            var result = _validator.Validate(cardCategory);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyDescription_ReturnsFailure(string? description)
        {
            var cardCategory = new CardCategory
            {
                Name = "Test Category",
                Description = description!,
                BorderColor = "#FF0000"
            };

            var result = _validator.Validate(cardCategory);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Description");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyBorderColor_ReturnsFailure(string? borderColor)
        {
            var cardCategory = new CardCategory
            {
                Name = "Test Category",
                Description = "Test Description",
                BorderColor = borderColor!
            };

            var result = _validator.Validate(cardCategory);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BorderColor");
        }

        [Fact]
        public void Validate_MultipleErrors_ReturnsAllFailures()
        {
            var cardCategory = new CardCategory
            {
                Name = "",
                Description = "",
                BorderColor = ""
            };

            var result = _validator.Validate(cardCategory);

            Assert.False(result.IsValid);
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
            Assert.Contains(result.Errors, e => e.PropertyName == "Description");
            Assert.Contains(result.Errors, e => e.PropertyName == "BorderColor");
        }
    }
}
