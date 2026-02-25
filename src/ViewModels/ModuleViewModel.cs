using System.ComponentModel.DataAnnotations;

namespace KidFit.ViewModels
{
    public class CreateModuleViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "";
        [Required(ErrorMessage = "Core slot is required")]
        public int CoreSlot { get; set; }
        [Required(ErrorMessage = "Total slot is required")]
        public int TotalSlot { get; set; }
    }

    public class ModuleViewModel
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "";
        [Required(ErrorMessage = "Core slot is required")]
        public int CoreSlot { get; set; }
        [Required(ErrorMessage = "Total slot is required")]
        public int TotalSlot { get; set; }
    }
}
