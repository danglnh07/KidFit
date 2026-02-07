namespace KidFit.Dtos.Responses
{
    public class ViewCardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public ViewCardCategoryDto CardCategory { get; set; } = new();

    }
}
