using FluentValidation;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using X.PagedList;

namespace KidFit.Services
{
    public class ModuleService(IUnitOfWork uow,
                               IValidator<Module> validator,
                               ILogger<ModuleService> logger)
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly IValidator<Module> _validator = validator;
        private readonly ILogger<ModuleService> _logger = logger;

        public async Task<bool> CreateModuleAsync(Module module)
        {
            // Model validation
            var validationResult = _validator.Validate(module);
            if (!validationResult.IsValid)
            {
                var message = "Failed to create module: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Create module
            await _uow.Repo<Module>().CreateAsync(module);
            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteModuleAsync(Guid id)
        {
            // Soft cascade associating lessons
            var deletedLesson = await _uow.Repo<Lesson>().BulkSoftDeleteAsync(l => l.ModuleId == id);
            _logger.LogInformation($"Module {id} is deleted -> Soft delete {deletedLesson} associating lessons");

            // Soft delete module
            var result = await _uow.Repo<Module>().SoftDeleteAsync(id);
            if (!result)
            {
                throw NotFoundException.Create(typeof(Module).Name);
            }

            return await _uow.SaveChangesAsync() > 0;
        }

        public async Task<Module?> GetModuleAsync(Guid id)
        {
            var module = await _uow.Repo<Module>().GetByIdAsync(id);
            return module;
        }

        public async Task<IPagedList<Module>> GetAllModulesAsync(QueryParam<Module> param)
        {
            // Get the paged list from repo
            var modules = await _uow.Repo<Module>().GetAllAsync(param);
            return modules;
        }

        public async Task<bool> UpdateModuleAsync(Module module)
        {
            // Model validation
            var validationResult = _validator.Validate(module);
            if (!validationResult.IsValid)
            {
                var message = "Failed to update module: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Update database entity
            _uow.Repo<Module>().Update(module);
            return await _uow.SaveChangesAsync() > 0;
        }
    }
}
