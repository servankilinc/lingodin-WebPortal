using Core.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using RestSharp;
using WebApp.Models.Auth;
using WebApp.Models.Dtos.RoleDtos;
using WebApp.Models.Dtos.UserDtos;
using WebApp.Models.DynamicQuery;
using WebApp.Models.Pagination;
using WebApp.Models.ViewModels;
using WebApp.Utils;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin, Authorized")]
public class UserController : Controller
{
    private readonly RestClient _client;
    private readonly IValidator<UserCreateDto> _signupDtoValidators;
    public UserController(RestClient client, IValidator<UserCreateDto> signupDtoValidators)
    {
        _client = client;
        _signupDtoValidators = signupDtoValidators;
    }


    [Authorize(Roles = "Admin, Authorized")]
    public async Task<IActionResult> Index(UserIndexFilterModel filterModel, int page = 0, int size = 5, string sortBy = "1")
    {
        FSPModel fSPModel = GenerateFSPModel(filterModel, page, size, sortBy);

        var request = new RestRequest("api/User/GetUserListByDetail", Method.Post);
        request.AddJsonBody(fSPModel);
        var response = await _client.ExecuteAsync<Paginate<UserDetailResponseDto>>(request);

        UserIndexViewModel viewModel = new() { FilterModel = filterModel };
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseData = JsonConvert.DeserializeObject<Paginate<UserDetailResponseDto>>(response.Content!);
            
            // filter for selected role
            if(filterModel.RoleIdList != null && filterModel.RoleIdList.Length > 0 && responseData != null && responseData.Items != null)
                responseData.Items = responseData.Items.Where(u => u.Roles != null && u.Roles.Any(r => filterModel.RoleIdList!.Contains(r.Id))).ToList();
        
            viewModel.Resource = responseData;
        }

        AddSortListToViewBag(sortBy);
        await AddRoleListToViewBag();
        return View(viewModel);
    }


    [Authorize(Roles = "Admin")]
    public IActionResult CreateAuthorized()
    {
        return View();
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAuthorized(UserCreateDto userCreateDto)
    {
        userCreateDto.AutheticatorType = AutheticatorType.Email;
        var validationResult = await _signupDtoValidators.ValidateAsync(userCreateDto);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(this.ModelState);
            return View(userCreateDto);
        }
         
        var request = new RestRequest("api/Auth/CreateAuthorized", Method.Post);
        request.AddJsonBody(userCreateDto);
        var response = await _client.ExecuteAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK) 
            return RedirectToAction("Index", "User");

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return View(userCreateDto);
    }



    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeUserAccountVerifyStatus(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("The userId cannot be empty.", nameof(userId));

        var request = new RestRequest("api/Auth/ChangeUserAccountVerifyStatus", Method.Get);
        request.AddQueryParameter("userId", userId);
        await _client.GetAsync(request);
        return RedirectToAction("Index", "User");
    }



    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("The userId cannot be empty.", nameof(userId));

        var request = new RestRequest("api/User/Delete", Method.Delete);
        request.AddQueryParameter("userId", userId);
        await _client.ExecuteAsync(request);
        return RedirectToAction("Index", "User");
    }

 

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUserAuthorization(Guid userId, string userName)
    {
        if (userId == Guid.Empty) throw new ArgumentException("The userId cannot be empty.", nameof(userId));

        var request = new RestRequest("api/Role/GetAllByUser", Method.Get);
        request.AddQueryParameter("userId", userId);
        var response = await _client.GetAsync(request);
        //var response = await _client.ExecuteAsync(request);
        var roleList = JsonConvert.DeserializeObject<List<RoleByUserModel>>(response.Content!);
        ViewBag.UserName = userName;
        return View(roleList);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddAuthorization(Guid userId, Guid roleId, string? userName)
    {
        if (userId == Guid.Empty) throw new ArgumentException("The userId cannot be empty.", nameof(userId));
        if (roleId == Guid.Empty) throw new ArgumentException("The roleId cannot be empty.", nameof(roleId));

        var request = new RestRequest("api/Role/AddRoleToUser", Method.Post);
        request.AddJsonBody(new RoleUserRequestDto() { RoleId = roleId, UserId = userId });
        await _client.ExecuteAsync(request);

        return RedirectToAction("EditUserAuthorization", "User", new { userId, userName } );
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAuthorization(Guid userId, Guid roleId, string? userName)
    {
        if (userId == Guid.Empty) throw new ArgumentException("The userId cannot be empty.", nameof(userId));
        if (roleId == Guid.Empty) throw new ArgumentException("The roleId cannot be empty.", nameof(roleId));

        var request = new RestRequest("api/Role/RemoveRoleFromUser", Method.Post);
        request.AddJsonBody(new RoleUserRequestDto() { RoleId = roleId, UserId = userId });
        await _client.ExecuteAsync(request);

        return RedirectToAction("EditUserAuthorization", "User", new { userId, userName });
    }


    // ------- HELPER METHODS... -------
    private void AddSortListToViewBag(string sortBy)
    {
        List<SelectListItem> sortItemList = new()
        {
            new(){ Text = "Alfabetik artan", Value= "1", Selected = sortBy == "1" },
            new(){ Text = "Alfabetik azalan", Value= "2", Selected = sortBy == "2" }
        };
        ViewBag.SortList = sortItemList;
    }

    private async Task AddRoleListToViewBag()
    {
        var request = new RestRequest("api/Role/GetAll", Method.Get);
        var roleList = await _client.GetAsync<List<RoleResponseDto>>(request);

        List<SelectListItem> roleSelectList = roleList!.Select(c => new SelectListItem(text: c.Name, c.Id.ToString())).ToList();
        ViewBag.RoleSelectList = roleSelectList;
    }


    private FSPModel GenerateFSPModel(UserIndexFilterModel filterModel, int page, int size, string sortBy)
    {
        Filter filter = new()
        {
            Operator = "base",
            Logic = "and",
            Filters = new()
        };
        if (!string.IsNullOrEmpty(filterModel.SearchName))
        {
            var nameQuery = new Filter
            {
                Field = "FullName",
                Operator = "contains",
                Value = filterModel.SearchName
            };

            filter.Filters.Add(nameQuery);
        }

        List<Sort> sortList = new();
        switch (sortBy)
        {
            case "1":
                sortList.Add(new Sort() { Field = "FullName", Dir = "asc" });
                break;
            case "2":
                sortList.Add(new Sort() { Field = "FullName", Dir = "desc" });
                break;
            default:
                sortList.Add(new Sort() { Field = "FullName", Dir = "asc" });
                break;
        }


        FSPModel fSPModel = new()
        {
            DynamicQuery = new() { Sort = sortList, Filter = filter },
            PagingRequest = new() { Page = page, PageSize = size }
        };

        return fSPModel;
    }
}
