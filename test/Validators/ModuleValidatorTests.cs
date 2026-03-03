using KidFit.Models;
using KidFit.Validators;

namespace KidFit.Tests.Validators
{
    public class ModuleValidatorTests
    {
        private readonly ModuleValidator _validator;

        public ModuleValidatorTests()
        {
            _validator = new ModuleValidator();
        }

        [Fact]
        public void Validate_ValidModule_ReturnsSuccess()
        {
            var module = new Module
            {
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            var result = _validator.Validate(module);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyName_ReturnsFailure(string? name)
        {
            var module = new Module
            {
                Name = name!,
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            var result = _validator.Validate(module);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyDescription_ReturnsFailure(string? description)
        {
            var module = new Module
            {
                Name = "Test Module",
                Description = description!,
                CoreSlot = 5,
                TotalSlot = 10
            };

            var result = _validator.Validate(module);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Description");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_InvalidCoreSlot_ReturnsFailure(int coreSlot)
        {
            var module = new Module
            {
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = coreSlot,
                TotalSlot = 10
            };

            var result = _validator.Validate(module);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CoreSlot");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_InvalidTotalSlot_ReturnsFailure(int totalSlot)
        {
            var module = new Module
            {
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = totalSlot
            };

            var result = _validator.Validate(module);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "TotalSlot");
        }

        [Fact]
        public void Validate_CoreSlotGreaterThanTotalSlot_ReturnsFailure()
        {
            var module = new Module
            {
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 15,
                TotalSlot = 10
            };

            var result = _validator.Validate(module);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CoreSlot");
        }

        [Fact]
        public void Validate_CoreSlotEqualToTotalSlot_ReturnsSuccess()
        {
            var module = new Module
            {
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 10,
                TotalSlot = 10
            };

            var result = _validator.Validate(module);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_CoreSlotLessThanTotalSlot_ReturnsSuccess()
        {
            var module = new Module
            {
                Name = "Test Module",
                Description = "Test Description",
                CoreSlot = 5,
                TotalSlot = 10
            };

            var result = _validator.Validate(module);

            Assert.True(result.IsValid);
        }
    }
}
