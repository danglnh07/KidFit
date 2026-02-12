using KidFit.Shared.Constants;

namespace KidFit.Dtos
{
    public class CreateLessonDto
    {
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
        public Guid ModuleId { get; set; }
        public Year Year { get; set; }
        public List<Guid> CardIds { get; set; } = [];
    }

    public class ViewLessonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
        public Year Year { get; set; }
        public ViewModuleDto Module { get; set; } = new();
        public List<ViewCardDto> Cards { get; set; } = [];
    }

    public class UpdateLessonDto
    {
        public string? Name { get; set; } = "";
        public string? Content { get; set; } = "";
        public Guid? ModuleId { get; set; } = null;
        public Year? Year { get; set; } = null;
        public List<Guid>? CardIds { get; set; } = [];
    }
}
