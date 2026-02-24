using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KidFit.ViewModels
{
    public class CategoryOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class CreateCardViewModel
    {
        [ValidateNever]
        public IEnumerable<CategoryOption> AvailableCategories { get; set; } = null!;

        [Required]
        [NotNull]
        public string Name { get; set; } = "";
        [Required]
        [NotNull]
        public string Description { get; set; } = "";
        [Required]
        [NotNull]
        public string Image { get; set; } = "";
        [Required]
        [NotNull]
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

        [Required]
        [NotNull]
        public Guid Id { get; set; }
        [Required]
        [NotNull]
        public string Name { get; set; } = "";
        [Required]
        [NotNull]
        public string Description { get; set; } = "";
        [Required]
        [NotNull]
        public string Image { get; set; } = "";
        [Required]
        [NotNull]
        public Guid CategoryId { get; set; }
    }
}
