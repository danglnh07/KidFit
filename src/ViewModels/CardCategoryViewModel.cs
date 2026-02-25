using System.ComponentModel.DataAnnotations;

namespace KidFit.ViewModels
{
    public class CreateCardCategoryViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "";
        [Required(ErrorMessage = "Border color is required")]
        public string BorderColor { get; set; } = "";
    }

    public class CardCategoryViewModel
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "";
        [Required(ErrorMessage = "Border color is required")]
        public string BorderColor { get; set; } = "";
    }
}
