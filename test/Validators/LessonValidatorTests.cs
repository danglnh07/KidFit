using KidFit.Models;
using KidFit.Shared.Constants;
using KidFit.Validators;

namespace KidFit.Tests.Validators
{
    public class LessonValidatorTests
    {
        private readonly LessonValidator _validator;

        public LessonValidatorTests()
        {
            _validator = new LessonValidator();
        }

        [Fact]
        public void Validate_ValidLesson_ReturnsSuccess()
        {
            var lesson = new Lesson
            {
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = Guid.NewGuid(),
                Year = Year.KINDERGARTEN,
                Cards = [new Card()]
            };

            var result = _validator.Validate(lesson);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyName_ReturnsFailure(string? name)
        {
            var lesson = new Lesson
            {
                Name = name!,
                Content = "Test Content",
                ModuleId = Guid.NewGuid(),
                Year = Year.KINDERGARTEN,
                Cards = [new Card()]
            };

            var result = _validator.Validate(lesson);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyContent_ReturnsFailure(string? content)
        {
            var lesson = new Lesson
            {
                Name = "Test Lesson",
                Content = content!,
                ModuleId = Guid.NewGuid(),
                Year = Year.KINDERGARTEN,
                Cards = [new Card()]
            };

            var result = _validator.Validate(lesson);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Content");
        }

        [Fact]
        public void Validate_EmptyModuleId_ReturnsFailure()
        {
            var lesson = new Lesson
            {
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = Guid.Empty,
                Year = Year.KINDERGARTEN,
                Cards = [new Card()]
            };

            var result = _validator.Validate(lesson);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ModuleId");
        }

        [Fact]
        public void Validate_EmptyYear_ReturnsFailure()
        {
            var lesson = new Lesson
            {
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = Guid.NewGuid(),
                Year = 0,  // Default/empty enum value
                Cards = [new Card()]
            };

            var result = _validator.Validate(lesson);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Year");
        }

        [Fact]
        public void Validate_EmptyCards_ReturnsFailure()
        {
            var lesson = new Lesson
            {
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = Guid.NewGuid(),
                Year = Year.KINDERGARTEN,
                Cards = []
            };

            var result = _validator.Validate(lesson);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Cards");
        }

        [Fact]
        public void Validate_NullCards_ReturnsFailure()
        {
            var lesson = new Lesson
            {
                Name = "Test Lesson",
                Content = "Test Content",
                ModuleId = Guid.NewGuid(),
                Year = Year.KINDERGARTEN,
                Cards = null!
            };

            var result = _validator.Validate(lesson);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Cards");
        }
    }
}
