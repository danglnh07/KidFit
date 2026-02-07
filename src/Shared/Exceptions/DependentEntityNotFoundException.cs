namespace KidFit.Shared.Exceptions
{
    public class DependentEntityNotFoundException(string msg) : Exception(msg)
    {
        public List<Guid>? MissingIds { get; set; }

        public DependentEntityNotFoundException(string entityName, List<Guid> missingIds)
            : this($"Some {entityName} IDs do not exist in database: {string.Join(", ", missingIds)}")
        {
            MissingIds = missingIds;
        }
    }
}
