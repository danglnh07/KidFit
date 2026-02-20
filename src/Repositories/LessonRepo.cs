using KidFit.Data;
using KidFit.Models;
using KidFit.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace KidFit.Repositories
{
    public class LessonRepo(AppDbContext context) : GenericRepo<Lesson>(context)
    {
        public async Task<Lesson?> GetByIdWithNestedDataAsync(Guid id)
        {
            return await _context.Lessons
                .Include(lesson => lesson.Cards)
                .Include(lesson => lesson.Module)
                .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<IPagedList<Lesson>> GetAllWithNestedDataAsync(QueryParam<Lesson> param)
        {
            IQueryable<Lesson> query = _context.Lessons
                .Include(lesson => lesson.Cards)
                .Include(lesson => lesson.Module);

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
