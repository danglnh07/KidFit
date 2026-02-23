using FluentValidation;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using X.PagedList;

namespace KidFit.Services
{
    public class CardCategoryService(IUnitOfWork uow, IValidator<CardCategory> validator, ILogger<CardCategoryService> logger)
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IValidator<CardCategory> _validator = validator;
        private readonly ILogger<CardCategoryService> _logger = logger;

        public async Task<bool> CreateCardCategoryAsync(CardCategory category)
        {
            // Model validation
            var validationResult = _validator.Validate(category);
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

        public async Task<bool> DeleteCardCategoryAsync(Guid id)
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

        public async Task<CardCategory?> GetCardCategoryAsync(Guid id)
        {
            var category = await _uow.Repo<CardCategory>().GetByIdAsync(id);
            return category;
        }

        public async Task<IPagedList<CardCategory>> GetAllCardCategoriesAsync(QueryParam<CardCategory> param)
        {
            // Get the paged list from repo
            var categories = await _uow.Repo<CardCategory>().GetAllAsync(param);
            return categories;
        }

        // This method will just check if the new data is valid and then perform the update.
        // The entity passed should be fetched from database.
        public async Task<bool> UpdateCardCategoryAsync(CardCategory category)
        {
            // Validation: check if new card category is valid
            var validationResult = _validator.Validate(category);
            if (!validationResult.IsValid)
            {
                var message = "Failed to update card category: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Update database entity
            _uow.Repo<CardCategory>().Update(category);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
