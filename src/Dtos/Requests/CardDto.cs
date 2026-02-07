namespace KidFit.Dtos.Requests
{
    public class CreateCardDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public Guid CategoryId { get; set; }
    }

    public class UpdateCardDto
    {
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? Image { get; set; } = null;
        public Guid? CategoryId { get; set; } = null;
    }
}
