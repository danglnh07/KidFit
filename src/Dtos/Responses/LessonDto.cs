using KidFit.Shared.Constants;

namespace KidFit.Dtos.Responses
{
    public class ViewLessonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
        public Year Year { get; set; }
        public ViewModuleDto Module { get; set; } = new();
        public List<ViewCardDto> Cards { get; set; } = [];
    }
}
