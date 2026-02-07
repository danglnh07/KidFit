namespace KidFit.Models
{
    public class ModelBase
    {
        public Guid Id { get; set; }
        public DateTimeOffset TimeCreated { get; set; }
        public DateTimeOffset TimeUpdated { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
