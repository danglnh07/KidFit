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
    public class ModuleService(
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<Module> moduleValidator,
            ModuleQueryParamValidation queryParamValidator,
            ILogger<ModuleService> logger)
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly IValidator<Module> _moduleValidator = moduleValidator;
        private readonly ModuleQueryParamValidation _queryParamValidator = queryParamValidator;
        private readonly ILogger<ModuleService> _logger = logger;

        public async Task<bool> CreateModule(CreateModuleDto req)
        {
            // Map from DTO to model
            var card = _mapper.Map<Module>(req);

            // Model validation
            var validationResult = _moduleValidator.Validate(card);
            if (!validationResult.IsValid)
            {
                throw new Shared.Exceptions.ValidationException(validationResult.Errors);
            }

            // Create module
            await _uow.Repo<Module>().CreateAsync(card);
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteModule(Guid id)
        {
            // Soft cascade associating lessons
            var deletedCard = await _uow.Repo<Lesson>().BulkSoftDeleteAsync(l => l.ModuleId == id);
            _logger.LogInformation($"Module {id} is deleted -> Soft delete {deletedCard} associating lessons");

            // Soft delete module
            var result = await _uow.Repo<Module>().SoftDeleteAsync(id);
            if (!result)
            {
                throw new NotFoundException($"Failed to soft delete module {id}: ID not found");
            }

            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<ViewModuleDto?> GetModule(Guid id)
        {
            var module = await _uow.Repo<Module>().GetByIdAsync(id);
            if (module is null) return null;
            return _mapper.Map<ViewModuleDto>(module);
        }

        public async Task<IPagedList<ViewModuleDto>> GetModules(QueryParamDto queryParam)
        {
            // Validate page and size 
            var queryParamValidationResult = _queryParamValidator.Validate(queryParam);
            if (!queryParamValidationResult.IsValid)
            {
                throw new Shared.Exceptions.ValidationException(queryParamValidationResult.Errors);
            }

            // Get the paged list from repo
            var modules = await _uow.Repo<Module>().GetAllAsync(new(queryParam));
            return _mapper.Map<IPagedList<ViewModuleDto>>(modules);
        }

        public async Task<bool> UpdateModule(Guid id, UpdateModuleDto req)
        {
            // Get entity from database by ID
            var dbModule = await _uow.Repo<Module>().GetByIdAsync(id) ?? throw new NotFoundException($"Module {id} not found");

            // Map from request DTO to database entity
            _mapper.Map(req, dbModule);

            // Model validation
            var validationResult = _moduleValidator.Validate(dbModule);
            if (!validationResult.IsValid)
            {
                throw new Shared.Exceptions.ValidationException(validationResult.Errors);
            }

            // Update database entity
            _uow.Repo<Module>().Update(dbModule);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
