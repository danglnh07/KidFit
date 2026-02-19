using KidFit.Dtos;
using KidFit.Models;
using KidFit.Validators;

namespace KidFit.Tests.Validators
{
    public class QueryParamValidatorTests
    {
        private readonly QueryParamValidator<Module> _validator;

        public QueryParamValidatorTests()
        {
            _validator = new QueryParamValidator<Module>();
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(1, 1)]
        [InlineData(100, 50)]
        public void Validate_ValidQueryParams_ReturnsSuccess(int page, int size)
        {
            var queryParam = new QueryParamDto
            {
                Page = page,
                Size = size
            };

            var result = _validator.Validate(queryParam);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(0, 10, "Page")]
        [InlineData(-1, 10, "Page")]
        public void Validate_InvalidPage_ReturnsFailure(int page, int size, string expectedProperty)
        {
            var queryParam = new QueryParamDto
            {
                Page = page,
                Size = size
            };

            var result = _validator.Validate(queryParam);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == expectedProperty);
        }

        [Theory]
        [InlineData(1, 0, "Size")]
        [InlineData(1, -1, "Size")]
        [InlineData(1, 51, "Size")]
        [InlineData(1, 100, "Size")]
        public void Validate_InvalidSize_ReturnsFailure(int page, int size, string expectedProperty)
        {
            var queryParam = new QueryParamDto
            {
                Page = page,
                Size = size
            };

            var result = _validator.Validate(queryParam);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == expectedProperty);
        }

        [Fact]
        public void Validate_ValidOrderBy_ReturnsSuccess()
        {
            var queryParam = new QueryParamDto
            {
                Page = 1,
                Size = 10,
                OrderBy = "Name"
            };

            var result = _validator.Validate(queryParam);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ValidOrderByCaseInsensitive_ReturnsSuccess()
        {
            var queryParam = new QueryParamDto
            {
                Page = 1,
                Size = 10,
                OrderBy = "name"
            };

            var result = _validator.Validate(queryParam);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_InvalidOrderBy_ReturnsFailure()
        {
            var queryParam = new QueryParamDto
            {
                Page = 1,
                Size = 10,
                OrderBy = "NonExistentProperty"
            };

            var result = _validator.Validate(queryParam);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "OrderBy");
        }

        [Fact]
        public void Validate_NullOrderBy_ReturnsSuccess()
        {
            var queryParam = new QueryParamDto
            {
                Page = 1,
                Size = 10,
                OrderBy = null
            };

            var result = _validator.Validate(queryParam);

            Assert.True(result.IsValid);
        }
    }
}
