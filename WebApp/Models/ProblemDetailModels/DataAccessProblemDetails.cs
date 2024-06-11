using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebApp.Models.ProblemDetailModels;

public class DataAccessProblemDetails : ProblemDetails
{
    public override string ToString() => JsonSerializer.Serialize(this);
}
