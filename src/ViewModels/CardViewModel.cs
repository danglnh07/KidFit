using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KidFit.ViewModels
{
    public class CategoryOption(Guid id, string name)
    {
        public Guid Id { get; set; } = id;
        public string Name { get; set; } = name;
    }

    public class CreateCardViewModel
    {
        [ValidateNever]
        public IEnumerable<CategoryOption> AvailableCategories { get; set; } = null!;

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "";
        [Required(ErrorMessage = "Image is required")]
        public string Image { get; set; } = "";
        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }
    }

    public class CardViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public Guid CategoryId { get; set; }
        public CardCategoryViewModel Category { get; set; } = null!;
    }

    public class UpdateCardViewModel
    {
        [ValidateNever]
        public IEnumerable<CategoryOption> AvailableCategories { get; set; } = null!;
        [ValidateNever]
        public CardCategoryViewModel Category { get; set; } = null!;

        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = "";
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "";
        [Required(ErrorMessage = "Image is required")]
        public string Image { get; set; } = "";
        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }
    }
}
