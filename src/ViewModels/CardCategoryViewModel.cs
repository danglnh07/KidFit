using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace KidFit.ViewModels
{
    public class CreateCardCategoryViewModel
    {
        [Required]
        [NotNull]
        public string Name { get; set; } = "";
        [Required]
        [NotNull]
        public string Description { get; set; } = "";
        [Required]
        [NotNull]
        public string BorderColor { get; set; } = "";
    }

    public class CardCategoryViewModel
    {
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
        public string BorderColor { get; set; } = "";
    }
}
