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
    public class CardCategoryService(IUnitOfWork uow,
                                     IMapper mapper,
                                     IValidator<CardCategory> cardCategoryValidator,
                                     ILogger<CardCategoryService> logger)
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<CardCategory> _cardCategoryvalidator = cardCategoryValidator;
        private readonly IValidator<QueryParamDto> _queryParamValidator = new QueryParamValidator<CardCategory>();
        private readonly ILogger<CardCategoryService> _logger = logger;

        public async Task<bool> CreateCardCategory(CreateCardCategoryDto req)
        {
            // Map from DTO to model 
            var category = _mapper.Map<CardCategory>(req);

            // Model validation
            var validationResult = _cardCategoryvalidator.Validate(category);
            if (!validationResult.IsValid)
            {
                var message = "Failed to create card category: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Create card category
            await _uow.Repo<CardCategory>().CreateAsync(category);
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCardCategory(Guid id)
        {
            // Soft cascade associating card 
            var deletedCards = await _uow.Repo<Card>().BulkSoftDeleteAsync(c => c.CategoryId == id);
            _logger.LogInformation($"Card category {id} is deleted -> Soft delete {deletedCards} associating cards");

            // Soft delete card category
            var result = await _uow.Repo<CardCategory>().SoftDeleteAsync(id);
            if (!result)
            {
                throw NotFoundException.Create(typeof(CardCategory).Name);
            }

            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<ViewCardCategoryDto?> GetCardCategory(Guid id)
        {
            var category = await _uow.Repo<CardCategory>().GetByIdAsync(id);
            if (category is null) return null;
            return _mapper.Map<ViewCardCategoryDto>(category);
        }

        public async Task<IPagedList<ViewCardCategoryDto>> GetAllCardCategories(QueryParamDto queryParam)
        {
            // Validation against query param 
            var queryParamValidationResult = _queryParamValidator.Validate(queryParam);
            if (!queryParamValidationResult.IsValid)
            {
                var message = "Failed to get all card categories: query param validation failed";
                List<string> errors = [.. queryParamValidationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Get the paged list from repo 
            var categories = await _uow.Repo<CardCategory>().GetAllAsync(new(queryParam));
            return _mapper.Map<IPagedList<ViewCardCategoryDto>>(categories);
        }

        public async Task<bool> UpdateCardCategory(Guid id, UpdateCardCategoryDto req)
        {
            // Get entity from database by ID
            var dbCardCategory = await _uow.Repo<CardCategory>().GetByIdAsync(id) ?? throw NotFoundException.Create(typeof(CardCategory).Name);

            // Map from request DTO to database entity 
            _mapper.Map(req, dbCardCategory);

            // Validate new data 
            var validationResult = _cardCategoryvalidator.Validate(dbCardCategory);
            if (!validationResult.IsValid)
            {
                var message = "Failed to update card category: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Update database entity
            _uow.Repo<CardCategory>().Update(dbCardCategory);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
