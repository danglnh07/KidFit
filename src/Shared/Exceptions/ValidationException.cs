using System.Text;

namespace KidFit.Shared.Exceptions
{
    public class ValidationException : Exception
    {
        private ValidationException(string message) : base(message) { }

        public static ValidationException Create(string message, List<string> errors)
        {
            var msg = new StringBuilder();
            msg.AppendLine($"Validation failed: {message}");
            errors.ForEach(e => msg.AppendLine($"- {e}"));
            return new(msg.ToString());
        }
    }
}
