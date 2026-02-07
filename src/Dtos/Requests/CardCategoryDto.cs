namespace KidFit.Dtos.Requests
{
    public class CreateCardCategoryDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string BorderColor { get; set; } = "";
    }

    public class UpdateCardCategoryDto
    {
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? BorderColor { get; set; } = null;
    }
}
