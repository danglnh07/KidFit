namespace KidFit.Dtos
{
    public class CreateCardDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public Guid CategoryId { get; set; }
    }

    public class ViewCardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public ViewCardCategoryDto CardCategory { get; set; } = new();

    }
    public class UpdateCardDto
    {
        public string? Name { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? Image { get; set; } = null;
        public Guid? CategoryId { get; set; } = null;
    }
}
