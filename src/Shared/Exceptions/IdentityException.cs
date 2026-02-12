using System.Text;
using Microsoft.AspNetCore.Identity;

namespace KidFit.Shared.Exceptions
{
    public class IdentityException : Exception
    {
        private IdentityException(string message) : base(message) { }

        public static IdentityException Create(string message, IEnumerable<IdentityError> errors)
        {
            var msg = new StringBuilder();
            msg.AppendLine($"Identity exception: {message}");

            foreach (var error in errors)
            {
                msg.AppendLine($"{error.Code}: {error.Description}");
            }

            return new IdentityException(msg.ToString());
        }

        public static IdentityException Create(IEnumerable<IdentityError> errors)
        {
            return Create("", errors);
        }

        public static IdentityException Create(string message)
        {
            return Create(message, []);
        }
    }
}
