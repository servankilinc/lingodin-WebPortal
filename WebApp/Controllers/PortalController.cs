using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebApp.Controllers;

[Authorize(Roles = "Admin, Authorized")]
public class PortalController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
