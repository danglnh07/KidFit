using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KidFit.Shared.Util
{
    public static class Util
    {
        public static List<string> GetModelValidationError(ModelStateDictionary modelState)
        {
            var errors = modelState.Where(m => m.Value is not null && m.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

            var errorList = new List<string>();
            foreach (var error in errors)
            {
                errorList.Add($"{error.Key}: {string.Join(", ", error.Value)}");
            }

            return errorList;
        }

        public static int CalculateTotalPages(int totalCount, int size)
        {
            return (int)Math.Ceiling((double)totalCount / size);
        }
    }
}
