namespace KidFit.Models
{
    public class Module : ModelBase
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int CoreSlot { get; set; }
        public int TotalSlot { get; set; }
    }
}
