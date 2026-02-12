using AutoMapper;
using FluentValidation;
using KidFit.Dtos;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using X.PagedList;

namespace KidFit.Services
{
    public class LessonService(
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<Lesson> lessonValidator,
            ILogger<LessonService> logger)
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<Lesson> _lessonValidator = lessonValidator;
        private readonly IValidator<QueryParamDto> _queryParamValidator = new QueryParamValidator<Lesson>();
        private readonly ILogger<LessonService> _logger = logger;

        private async Task<bool> IsModuleExist(Guid moduleId)
        {
            var module = await _uow.Repo<Module>().GetByIdAsync(moduleId);
            return module is not null;
        }

        private async Task<int> AreAnyCardsMissing(List<Guid> cardIds)
        {
            return await _uow.Repo<Card>().CountExists(cardIds);
        }

        public async Task<bool> CreateLesson(CreateLessonDto req)
        {
            // Validation: check if module exists yet
            if (!await IsModuleExist(req.ModuleId))
            {
                throw DependentEntityNotFoundException.Create(typeof(Module).Name);
            }

            // Validation: check if all cards exists yet
            var dbCardCount = await AreAnyCardsMissing(req.CardIds);
            if (dbCardCount != req.CardIds.Count)
            {
                throw DependentEntityNotFoundException.Create(typeof(Card).Name, req.CardIds.Count - dbCardCount);
            }

            // Map from DTO to model
            var lesson = _mapper.Map<Lesson>(req);

            // Model validation 
            var validationResult = _lessonValidator.Validate(lesson);
            if (!validationResult.IsValid)
            {
                var message = "Failed to create lesson: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Create lesson 
            await _uow.Repo<Lesson>().CreateAsync(lesson);
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteLesson(Guid id)
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

        public async Task<ViewLessonDto?> GetLesson(Guid id)
        {
            var lesson = await _uow.Repo<Lesson>().GetByIdAsync(id);
            if (lesson is null) return null;
            return _mapper.Map<ViewLessonDto>(lesson);
        }

        public async Task<IPagedList<ViewLessonDto>> GetAllLessons(QueryParamDto queryParam)
        {
            // Validation page and size
            var queryParamValidationResult = _queryParamValidator.Validate(queryParam);
            if (!queryParamValidationResult.IsValid)
            {
                var message = "Failed to get all lessons: query param validation failed";
                List<string> errors = [.. queryParamValidationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Get the paged list from repo
            var lessons = await _uow.Repo<Lesson>().GetAllAsync(new(queryParam));
            return _mapper.Map<IPagedList<ViewLessonDto>>(lessons);
        }

        public async Task<bool> UpdateLesson(Guid id, UpdateLessonDto req)
        {
            // Get entity from database by ID
            var dbLesson = await _uow.Repo<Lesson>().GetByIdAsync(id) ?? throw NotFoundException.Create(typeof(Lesson).Name);

            // Validation: check if module exists yet
            if (
                    req.ModuleId is not null &&
                    req.ModuleId != Guid.Empty &&
                    await IsModuleExist((Guid)req.ModuleId) == false)
            {
                throw DependentEntityNotFoundException.Create(typeof(Module).Name);
            }

            // Validation: check if all cards exists yet
            if (req.CardIds is not null && req.CardIds.Count > 0)
            {
                var dbCardCount = await AreAnyCardsMissing(req.CardIds);
                if (dbCardCount != req.CardIds.Count)
                {
                    throw DependentEntityNotFoundException.Create(typeof(Card).Name, req.CardIds.Count - dbCardCount);
                }
            }

            // Map from request DTO to database entity
            _mapper.Map(req, dbLesson);

            // Model validation
            var validationResult = _lessonValidator.Validate(dbLesson);
            if (!validationResult.IsValid)
            {
                var message = "Failed to update lesson: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Update database entity
            _uow.Repo<Lesson>().Update(dbLesson);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
