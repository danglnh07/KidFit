namespace KidFit.Dtos.Responses
{
    public class ViewModuleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int CoreSlot { get; set; }
        public int TotalSlot { get; set; }
    }
}
