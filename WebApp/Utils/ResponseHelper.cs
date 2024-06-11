using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using WebApp.Models.ProblemDetailModels;

namespace WebApp.Utils;

public static class ResponseHelper
{
    public static void HandleResponseError(RestResponse response, ModelStateDictionary modelState)
    {
        if (response.Content == null)
        {
            modelState.AddModelError(string.Empty, response.ErrorMessage ?? "An error occurred, please try again!");
        }
        else
        {
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(response.Content);
            if (problemDetails == null) modelState.AddModelError(string.Empty, response.Content ?? "An error occurred, please try again!");
            
            if (response.StatusCode == HttpStatusCode.BadRequest)  // Bad Request (Validation & Business)
            {
                if (problemDetails?.Type == ProblemDetailTypes.Validation.ToString()) // validation error 
                {
                    var validationProblemDetails = JsonConvert.DeserializeObject<Models.ProblemDetailModels.ValidationProblemDetails>(response.Content!)!;
                    validationProblemDetails.Errors?.AddToModelState(modelState);
                }
                else // business error 
                {
                    modelState.AddModelError(string.Empty, problemDetails?.Detail ?? "An error occurred, please try again!");
                }
            }
            else // Internal Server Error (DataAccess & Others)
            {
                modelState.AddModelError(string.Empty, problemDetails?.Detail ?? "An error occurred, please try again!");
            }
        }
    }
}
