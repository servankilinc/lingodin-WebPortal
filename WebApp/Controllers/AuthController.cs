using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using WebApp.Models.Dtos.UserDtos;
using WebApp.Models.ViewModels;
using WebApp.Utils;

namespace WebApp.Controllers;

public class AuthController : Controller
{
    private readonly RestClient _client;
    private readonly IValidator<LoginByEmailDto> _loginDtoValidators;
    public AuthController(RestClientOptions clientOptions, IValidator<LoginByEmailDto> loginDtoValidators)
    {
        _client = new RestClient(clientOptions);
        _loginDtoValidators = loginDtoValidators;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginByEmailDto loginByEmailDto)
    {
        var validationResult = await _loginDtoValidators.ValidateAsync(loginByEmailDto);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(this.ModelState);
            return View(loginByEmailDto);
        }

        var request = new RestRequest("api/Auth/Login", Method.Post);
        request.AddJsonBody(loginByEmailDto);
        var response = await _client.ExecuteAsync(request);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var userAuthResponse = JsonConvert.DeserializeObject<UserAuthResponseModel>(response.Content!);
            if (userAuthResponse == null || userAuthResponse.AccessToken == null || userAuthResponse.AccessToken?.Token == null)
            {
                ModelState.AddModelError(string.Empty, "Bir hata oluştu.");
                return View(loginByEmailDto);
            }

            Response.Cookies.Append("access_token", userAuthResponse.AccessToken.Token!, new CookieOptions()
            {
                HttpOnly = true, // XSS ataklarını önlemek için
                Secure = true, // HTTPS kullanıyorsanız
                SameSite = SameSiteMode.Strict, // CSRF ataklarını önlemek için
                Expires = userAuthResponse.AccessToken.Expiration // Token'ın süresi
            });
            return RedirectToAction("Index", "Home");
        }

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return View(loginByEmailDto);
    }



    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Delete("access_token");
        return View("Login");
    }
}
