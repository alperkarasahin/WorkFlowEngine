using FluentValidation;
using FluentValidation.Results;

namespace WorkFlowManager.Helper
{
    public class ValidationHelper
    {
        public static bool Validate<TModel, TValidator>(TModel model, TValidator validator,
            System.Web.Mvc.ModelStateDictionary modelState)
            where TValidator : AbstractValidator<TModel>
        {
            ValidationResult result = validator.Validate(model);

            foreach (var error in result.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return result.IsValid;
        }
    }
}
