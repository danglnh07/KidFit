namespace KidFit.Models
{
    public class ModelBase
    {
        public Guid Id { get; set; }
        public DateTimeOffset TimeCreated { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset TimeUpdated { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
