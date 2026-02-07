using KidFit.Shared.Constants;

namespace KidFit.Models
{
    public class Lesson : ModelBase
    {
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
        public Guid ModuleId { get; set; }
        public Module Module { get; set; } = new();
        public List<Card> Cards { get; set; } = [];
        public Year Year { get; set; }
    }
}
