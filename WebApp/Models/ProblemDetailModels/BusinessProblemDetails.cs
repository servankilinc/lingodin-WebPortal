using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebApp.Models.ProblemDetailModels;

public class BusinessProblemDetails : ProblemDetails
{
    public override string ToString() => JsonSerializer.Serialize(this);
}