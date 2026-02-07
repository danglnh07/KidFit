namespace KidFit.Dtos.Responses
{
    public class ViewCardCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string BorderColor { get; set; } = "";
    }
}
