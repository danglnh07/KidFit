namespace KidFit.Shared.Exceptions
{
    public class ForbiddenException : Exception
    {
        private ForbiddenException(string message) : base(message) { }

        public static ForbiddenException Create(string message)
        {
            return new(message);
        }
    }
}
