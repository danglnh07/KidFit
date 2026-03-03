using KidFit.Data;
using KidFit.Models;

namespace KidFit.Repositories
{
    public class CardCategoryRepo(AppDbContext context) : GenericRepo<CardCategory>(context)
    {
        public async Task<IEnumerable<(Guid id, string name)>> GetAllCardCategoriesWithNameOnlyAsync()
        {
            return _context.CardCategories.Select(card => new ValueTuple<Guid, string>(card.Id, card.Name)).ToList();
        }
    }
}
