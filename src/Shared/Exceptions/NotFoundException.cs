namespace KidFit.Shared.Exceptions
{
    public class NotFoundException : Exception
    {
        private NotFoundException(string message) : base(message) { }

        public static NotFoundException Create(string entity)
        {
            return new($"Entity {entity} not found");
        }
    }
}
