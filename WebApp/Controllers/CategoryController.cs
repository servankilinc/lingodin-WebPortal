using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WebApp.Models.Dtos.CategoryDtos;
using WebApp.Utils;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin, Authorized")]
public class CategoryController : Controller
{
    private readonly RestClient _client;
    private readonly IValidator<CategoryCreateDto> _categoryCreateValidator;
    private readonly IValidator<CategoryUpdateDto> _categoryUpdateValidator;
    private readonly IMapper _mapper;
    public CategoryController(RestClient client, IValidator<CategoryCreateDto> categoryCreateValidator, IValidator<CategoryUpdateDto> categoryUpdateValidator, IMapper mapper)
    {
        _client = client;
        _categoryCreateValidator = categoryCreateValidator;
        _categoryUpdateValidator = categoryUpdateValidator;
        _mapper = mapper;
    }


    public async Task<IActionResult> Index()
    { 
        var request = new RestRequest("api/Category/GetAll", Method.Get);
        var response = await _client.GetAsync<List<CategoryResponseDto>>(request);

        CategeryIndexViewModel viewModel = new()
        {
            CategoryList = response
        };
        return View(viewModel);
    }


    // CREATE
    public IActionResult Create()
    { 
        return View(); 
    }
 
    [HttpPost]
    public async Task<IActionResult> Create(CategoryCreateDto categoryCreateDto)
    {
        var validationResult = await _categoryCreateValidator.ValidateAsync(categoryCreateDto);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(this.ModelState);
            return View(categoryCreateDto);
        }
         
        var request = new RestRequest("api/Category/Insert", Method.Post);
        request.AddJsonBody(categoryCreateDto);
        var response = await _client.ExecuteAsync<CategoryResponseDto>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var createdCategory = JsonConvert.DeserializeObject<CategoryResponseDto>(response.Content!);
            return RedirectToAction("UpdateImage", "Category", new { categoryId = createdCategory!.Id });
        }

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return View(categoryCreateDto);
    }

    // DELETE 
    public async Task<IActionResult> Delete(Guid categoryId)
    { 
        var request = new RestRequest("api/Category/Delete", Method.Delete);
        request.AddQueryParameter("categoryId", categoryId);
        await _client.DeleteAsync(request);
        return RedirectToAction("Index", "Category");
    }

    // UPDATE INFO
    public async Task<IActionResult> Update(Guid categoryId)
    { 
        var request = new RestRequest("api/Category/Get", Method.Get);
        request.AddQueryParameter("categoryId", categoryId);
        var response = await _client.GetAsync<CategoryResponseDto>(request);

        CategoryUpdateDto updateDto = _mapper.Map<CategoryUpdateDto>(response); 
        return View(updateDto);
    }
    [HttpPost]
    public async Task<IActionResult> Update(CategoryUpdateDto categoryUpdateDto)
    {
        var validationResult = await _categoryUpdateValidator.ValidateAsync(categoryUpdateDto);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(this.ModelState);
            return View(categoryUpdateDto);
        }
         
        var request = new RestRequest("api/Category/Update", Method.Put);
        request.AddJsonBody(categoryUpdateDto);
        var response = await _client.ExecuteAsync<CategoryResponseDto>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return RedirectToAction("Index", "Category");

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return View(categoryUpdateDto);
    }

    // UPDATE IMAGE
    public async Task<IActionResult> UpdateImage(Guid categoryId)
    { 
        var request = new RestRequest("api/Category/Get", Method.Get);
        request.AddQueryParameter("categoryId", categoryId);
        var response = await _client.GetAsync<CategoryResponseDto>(request);

        return View(response);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateImage(IFormFile file, Guid categoryId)
    {
        if (file == null) this.ModelState.AddModelError("file", "File is required!");
        if (categoryId == Guid.Empty) this.ModelState.AddModelError("categoryId", "An error occurred, please try again!");
        if (!this.ModelState.IsValid) return RedirectToAction("UpdateImage", "Category", new { categoryId });
         
        var request = new RestRequest("api/Category/ImageUpdate", Method.Post);
        request.AddHeader("Content-Type", "multipart/form-data");
        request.AddQueryParameter("categoryId", categoryId);
        using (var ms = new MemoryStream())
        {
            await file!.CopyToAsync(ms);
            request.AddFile("file", ms.ToArray(), file.FileName, file.ContentType);
        }
        var response = await _client.ExecuteAsync<CategoryResponseDto>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return RedirectToAction("Index", "Category");

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return RedirectToAction("UpdateImage", "Category", new { categoryId });
    }

    // DELETE IMAGE
    public async Task<IActionResult> DeleteImage(Guid categoryId, string url)
    {
        if (categoryId == Guid.Empty) this.ModelState.AddModelError(string.Empty, "An error occurred, please try again!");
        if (string.IsNullOrEmpty(url)) this.ModelState.AddModelError("url", "An error occurred, please try again!");
        if (!this.ModelState.IsValid) return RedirectToAction("UpdateImage", "Category", new { categoryId });

        var request = new RestRequest("api/Category/ImageDelete", Method.Delete);
        request.AddQueryParameter("categoryId", categoryId);
        request.AddQueryParameter("url", url);
        var response = await _client.ExecuteAsync<CategoryResponseDto>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return RedirectToAction("UpdateImage", "Category", new { categoryId });

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return RedirectToAction("UpdateImage", "Category", new { categoryId });
    }
}