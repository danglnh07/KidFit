using KidFit.Models;
using KidFit.Validators;

namespace KidFit.Tests.Validators
{
    public class ApplicationUserValidatorTests
    {
        private readonly ApplicationUserValidator _validator;

        public ApplicationUserValidatorTests()
        {
            _validator = new ApplicationUserValidator();
        }

        [Fact]
        public void Validate_ValidUser_ReturnsSuccess()
        {
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };

            var result = _validator.Validate(user);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyUserName_ReturnsFailure(string? userName)
        {
            var user = new ApplicationUser
            {
                UserName = userName!,
                Email = "test@example.com"
            };

            var result = _validator.Validate(user);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "UserName");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyEmail_ReturnsFailure(string? email)
        {
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = email!
            };

            var result = _validator.Validate(user);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_EmptyUserNameAndEmail_ReturnsAllFailures()
        {
            var user = new ApplicationUser
            {
                UserName = "",
                Email = ""
            };

            var result = _validator.Validate(user);

            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.PropertyName == "UserName");
            Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_PasswordNotValidated_ReturnsSuccess()
        {
            // Validator doesn't check password - UserManager handles that
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = null
            };

            var result = _validator.Validate(user);

            Assert.True(result.IsValid);
        }
    }
}
