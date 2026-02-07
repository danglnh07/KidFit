using AutoMapper;
using FluentValidation;
using KidFit.Dtos.Requests;
using KidFit.Dtos.Responses;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Exceptions;
using KidFit.Validators;
using X.PagedList;

namespace KidFit.Services
{
    public class CardService(
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<Card> cardValidator,
            CardQueryParamValidation queryParamValidator,
            ILogger<CardService> logger)
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<Card> _cardValidator = cardValidator;
        private readonly CardQueryParamValidation _queryParamValidator = queryParamValidator;
        private readonly ILogger<CardService> _logger = logger;

        private async Task<bool> IsCardCategoryExists(Guid categoryId)
        {
            return await _uow.Repo<CardCategory>().GetByIdAsync(categoryId) is not null;
        }

        public async Task<bool> CreateCard(CreateCardDto req)
        {
            // Check if card category exists
            if (!await IsCardCategoryExists(req.CategoryId))
            {
                throw new NotFoundException($"Card category {req.CategoryId} not found");
            }

            // Map from DTO to model 
            var card = _mapper.Map<Card>(req);

            // Model validation
            var validationResult = _cardValidator.Validate(card);
            if (!validationResult.IsValid)
            {
                throw new Shared.Exceptions.ValidationException(validationResult.Errors);
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
                throw new NotFoundException($"Failed to soft delete card {id}: ID not found");
            }
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<ViewCardDto?> GetCard(Guid id)
        {
            var card = await _uow.Repo<Card>().GetByIdAsync(id);
            if (card is null) return null;
            return _mapper.Map<ViewCardDto>(card);
        }

        public async Task<IPagedList<ViewCardDto>> GetAllCards(QueryParamDto queryParam)
        {
            // Validation against query param
            var queryParamValidationResult = _queryParamValidator.Validate(queryParam);
            if (!queryParamValidationResult.IsValid)
            {
                throw new Shared.Exceptions.ValidationException(queryParamValidationResult.Errors);
            }

            // Get the paged list from repo
            var cards = await _uow.Repo<Card>().GetAllAsync(new(queryParam));
            return _mapper.Map<IPagedList<ViewCardDto>>(cards);
        }

        public async Task<bool> UpdateCard(Guid id, UpdateCardDto req)
        {
            // Get entity from database by ID 
            var card = await _uow.Repo<Card>().GetByIdAsync(id) ?? throw new NotFoundException($"Card {id} not found");

            // Validation: check if card category exists yet
            if (
                    req.CategoryId is not null &&
                    req.CategoryId != Guid.Empty &&
                    await IsCardCategoryExists((Guid)req.CategoryId) == false)
            {
                throw new NotFoundException($"Card category {req.CategoryId} not found");
            }

            // Map from request DTO to database entity
            _mapper.Map(req, card);

            // Model validation 
            var validationResult = _cardValidator.Validate(card);
            if (!validationResult.IsValid)
            {
                throw new Shared.Exceptions.ValidationException(validationResult.Errors);
            }

            // Update database entity
            _uow.Repo<Card>().Update(card);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
