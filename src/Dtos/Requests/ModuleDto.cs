namespace KidFit.Dtos.Requests
{
    public class CreateModuleDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int CoreSlot { get; set; }
        public int TotalSlot { get; set; }
    }

    public class UpdateModuleDto
    {
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
        public int? CoreSlot { get; set; } = null;
        public int? TotalSlot { get; set; } = null;
    }
}
