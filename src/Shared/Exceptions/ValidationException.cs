namespace KidFit.Shared.Exceptions
{
    public class ValidationException(List<FluentValidation.Results.ValidationFailure> errors) : Exception($"Valiation failed: {errors}")
    {
        public List<FluentValidation.Results.ValidationFailure> Errors { get; set; } = errors;
    }
}
