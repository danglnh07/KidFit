using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace KidFit.ViewModels
{
    public class CreateModuleViewModel
    {
        [Required]
        [NotNull]
        public string Name { get; set; } = "";
        [Required]
        [NotNull]
        public string Description { get; set; } = "";
        [Required]
        public int CoreSlot { get; set; }
        [Required]
        public int TotalSlot { get; set; }
    }

    public class ModuleViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int CoreSlot { get; set; }
        public int TotalSlot { get; set; }
    }
}
