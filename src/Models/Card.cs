namespace KidFit.Models
{
    public class Card : ModelBase
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public Guid CategoryId { get; set; }
        public CardCategory Category { get; set; } = new();
    }
}
