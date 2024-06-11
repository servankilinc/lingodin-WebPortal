using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApp.Utils;
public static class ExtensionsModelValidation
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
    
    public static void AddToModelState(this IEnumerable<ValidationFailure> list, ModelStateDictionary modelState)
    {
        foreach (var error in list)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}