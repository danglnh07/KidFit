using FluentValidation;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using X.PagedList;

namespace KidFit.Services
{
    public class CardService(IUnitOfWork uow, IValidator<Card> validator)
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IValidator<Card> _validator = validator;

        public async Task<bool> CreateCard(Card card)
        {
            // Model validation
            var validationResult = _validator.Validate(card);
            if (!validationResult.IsValid)
            {
                var message = "Failed to create card: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Check if card category exists
            if (!await _uow.Repo<CardCategory>().IsExistAsync(card.CategoryId))
            {
                throw DependentEntityNotFoundException.Create(typeof(CardCategory).Name);
            }

            // Create card
            await _uow.Repo<Card>().CreateAsync(card);
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCard(Guid id)
        {
            var result = await _uow.Repo<Card>().SoftDeleteAsync(id);
            if (!result)
            {
                throw NotFoundException.Create(typeof(Card).Name);
            }
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<Card?> GetCard(Guid id, bool allowIncludeNestedData = false)
        {
            // Use custom repo
            var repo = (CardRepo)_uow.Repo<Card>();

            // Return card
            return allowIncludeNestedData ? await repo.GetByIdWithNestedDataAsync(id) : await repo.GetByIdAsync(id);
        }

        public async Task<IPagedList<Card>> GetAllCards(QueryParam<Card> param, bool allowIncludeNestedData = false)
        {
            // Use custom repo
            var repo = (CardRepo)_uow.Repo<Card>();

            // Get the paged list from repo
            return allowIncludeNestedData ? await repo.GetAllWithNestedDataAsync(param) : await repo.GetAllAsync(param);
        }

        // This method will just check if the new data is valid and then perform the update.
        // The entity passed should be fetched from database.
        public async Task<bool> UpdateCard(Card card)
        {
            // Model validation
            var validationResult = _validator.Validate(card);
            if (!validationResult.IsValid)
            {
                var message = "Failed to update card: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Validation: check if card category exists yet
            if (!await _uow.Repo<CardCategory>().IsExistAsync(card.CategoryId))
            {
                throw DependentEntityNotFoundException.Create(typeof(CardCategory).Name);
            }

            // Update database entity
            _uow.Repo<Card>().Update(card);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
