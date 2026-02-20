using KidFit.Data;
using KidFit.Models;
using KidFit.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace KidFit.Repositories
{
    public class CardRepo(AppDbContext context) : GenericRepo<Card>(context)
    {
        public async Task<Card?> GetByIdWithNestedDataAsync(Guid id)
        {
            return await _context.Cards
                .Include(card => card.Category)
                .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<IPagedList<Card>> GetAllWithNestedDataAsync(QueryParam<Card> param)
        {
            IQueryable<Card> query = _context.Cards.Include(card => card.Category);

            if (param.OrderBy is not null && param.IsAsc is not null)
            {
                query = param.IsAsc == true
                    ? query.OrderBy(param.OrderBy)
                    : query.OrderByDescending(param.OrderBy);
            }

            return await query.ToPagedListAsync(param.Page, param.Size);
        }
    }
}
