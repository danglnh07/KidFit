using System.Text;

namespace KidFit.Shared.Exceptions
{
    public class DependentEntityNotFoundException : Exception
    {
        private DependentEntityNotFoundException(string message) : base(message) { }

        public static DependentEntityNotFoundException Create(string entity, int count)
        {
            var msg = new StringBuilder();

            msg.AppendLine("Dependent entities not exists");
            msg.AppendLine($"Entity: {entity}");
            msg.AppendLine($"Missing: {count}");

            return new(msg.ToString());
        }

        public static DependentEntityNotFoundException Create(string entity)
        {
            return Create(entity, 1);
        }
    }
}
