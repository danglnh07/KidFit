using FluentValidation;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using X.PagedList;

namespace KidFit.Services
{
    public class LessonService(IUnitOfWork uow, IValidator<Lesson> validator)
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IValidator<Lesson> _validator = validator;

        public async Task<bool> CreateLessonAsync(Lesson lesson)
        {
            // Model validation
            var validationResult = _validator.Validate(lesson);
            if (!validationResult.IsValid)
            {
                var message = "Failed to create lesson: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Validation: check if module exists yet
            if (!await _uow.Repo<Module>().IsExistAsync(lesson.ModuleId))
            {
                throw DependentEntityNotFoundException.Create(typeof(Module).Name);
            }

            // Validation: check if all cards exists yet
            var dbCardCount = await _uow.Repo<Card>().CountExistAsync(lesson.CardIds);
            if (dbCardCount != lesson.CardIds.Count)
            {
                throw DependentEntityNotFoundException.Create(typeof(Card).Name, lesson.CardIds.Count - dbCardCount);
            }

            // Create lesson
            await _uow.Repo<Lesson>().CreateAsync(lesson);
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteLessonAsync(Guid id)
        {
            // Since card is an indepedent entity from lesson, delete a lesson wouldn't
            // affect associating cards
            var result = await _uow.Repo<Lesson>().SoftDeleteAsync(id);
            if (!result)
            {
                throw NotFoundException.Create(typeof(Lesson).Name);
            }
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<Lesson?> GetLessonAsync(Guid id, bool allowIncludeNestedData = false)
        {
            // Use custom repo
            var repo = (LessonRepo)_uow.Repo<Lesson>();

            // Return lesson
            return allowIncludeNestedData ? await repo.GetByIdWithNestedDataAsync(id) : await repo.GetByIdAsync(id);
        }

        public async Task<IPagedList<Lesson>> GetAllLessonsAsync(QueryParam<Lesson> param, bool allowIncludeNestedData = false)
        {
            // Use custom repo
            var repo = (LessonRepo)_uow.Repo<Lesson>();

            // Get the paged list from repo
            return allowIncludeNestedData ? await repo.GetAllWithNestedDataAsync(param) : await repo.GetAllAsync(param);
        }

        public async Task<bool> UpdateLessonAsync(Lesson lesson)
        {
            // Model validation
            var validationResult = _validator.Validate(lesson);
            if (!validationResult.IsValid)
            {
                var message = "Failed to update lesson: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Validation: check if module exists yet
            if (!await _uow.Repo<Module>().IsExistAsync(lesson.ModuleId))
            {
                throw DependentEntityNotFoundException.Create(typeof(Module).Name);
            }

            // Validation: check if all cards exists yet
            if (lesson.CardIds is not null && lesson.CardIds.Count > 0)
            {
                var dbCardCount = await _uow.Repo<Card>().CountExistAsync(lesson.CardIds);
                if (dbCardCount != lesson.CardIds.Count)
                {
                    throw DependentEntityNotFoundException.Create(typeof(Card).Name, lesson.CardIds.Count - dbCardCount);
                }
            }

            // Update database entity
            _uow.Repo<Lesson>().Update(lesson);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
