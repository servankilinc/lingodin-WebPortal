using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebApp.Models.Dtos.CategoryDtos;
using WebApp.Models.Dtos.WordDtos;
using Newtonsoft.Json;
using WebApp.Utils;
using FluentValidation;
using WebApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Core.Utils;
using WebApp.Models.DynamicQuery;
using WebApp.Models.Pagination;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin, Authorized")]
public class WordController : Controller
{
    private readonly RestClient _client;
    private readonly IValidator<WordCreateDto> _wordCreateValidator;
    private readonly IValidator<WordUpdateDto> _wordUpdateValidator;
    private readonly IMapper _mapper;
    public WordController(RestClient client, IValidator<WordCreateDto> wordCreateValidator, IValidator<WordUpdateDto> wordUpdateValidator, IMapper mapper)
    {
        _client = client;
        _wordCreateValidator = wordCreateValidator;
        _wordUpdateValidator = wordUpdateValidator;
        _mapper = mapper;
    }
     
    public async Task<IActionResult> Index(WordIndexFilterModel filterModel, int page = 0, int size = 5, string sortBy = "1")
    {
        FSPModel fSPModel = GenerateFSPModel(filterModel, page, size, sortBy);

        var request = new RestRequest("api/Word/GetAll", Method.Post);
        request.AddJsonBody(fSPModel);
        var response = await _client.ExecuteAsync<Paginate<WordResponseDto>>(request);
         
        var resource = JsonConvert.DeserializeObject<Paginate<WordResponseDto>>(response.Content!);

        WordIndexViewModel viewModel = new()
        {
            FilterModel = filterModel,
            Resource = resource
        };

        AddSortListToViewBag(sortBy);
        await AddCategoryListToViewBag(Guid.Empty);

        return View(viewModel);
    }



    public async Task<IActionResult> List(Guid categoryId)
    {
        var request = new RestRequest("api/Word/GetWordsByCategory", Method.Get);
        request.AddQueryParameter("categoryId", categoryId);
        var response = await _client.GetAsync<List<WordResponseDto>>(request);

        ViewBag.CategoryId = categoryId;
        return View(response);
    }


    // CREATE
    public async Task<IActionResult> Create(Guid categoryId)
    {
        var request = new RestRequest("api/Category/Get", Method.Get);
        request.AddQueryParameter("categoryId", categoryId);
        var response = await _client.GetAsync<CategoryResponseDto>(request);
        WordCreateViewModel viewModel = new(category: response);
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(WordCreateDto wordCreateDto)
    {
        var validationResult = await _wordCreateValidator.ValidateAsync(wordCreateDto);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(this.ModelState);
            return View(wordCreateDto);
        }

        var request = new RestRequest("api/Word/Insert", Method.Post);
        request.AddJsonBody(wordCreateDto);
        var response = await _client.ExecuteAsync<WordResponseDto>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var createdWord= JsonConvert.DeserializeObject<WordResponseDto>(response.Content!);
            return RedirectToAction("UpdateImage", "Word", new { wordId = createdWord!.Id, isFlow = true });
        }

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return View(wordCreateDto);
    }

    // UPDATE INFO
    public async Task<IActionResult> Update(Guid wordId)
    {
        var requestWord = new RestRequest("api/Word/Get", Method.Get).AddQueryParameter("wordId", wordId);
        var word = await _client.GetAsync<WordResponseDto>(requestWord);
        WordUpdateDto updateDto = _mapper.Map<WordUpdateDto>(word);

        await AddCategoryListToViewBag(word!.CategoryId);

        return View(updateDto);
    }

    [HttpPost]
    public async Task<IActionResult> Update(WordUpdateDto wordUpdateDto)
    {
        var validationResult = await _wordUpdateValidator.ValidateAsync(wordUpdateDto);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(this.ModelState);
            await AddCategoryListToViewBag(wordUpdateDto!.CategoryId);
            return View(wordUpdateDto);
        }

        var request = new RestRequest("api/Word/Update", Method.Put).AddJsonBody(wordUpdateDto);
        var response = await _client.ExecuteAsync<WordResponseDto>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseDto = JsonConvert.DeserializeObject<WordResponseDto>(response.Content!);
            return RedirectToAction("List", "Word", new { categoryId = responseDto!.CategoryId});
        }

        ResponseHelper.HandleResponseError(response, this.ModelState);
        await AddCategoryListToViewBag(wordUpdateDto!.CategoryId);
        return View(wordUpdateDto);
    }


    // DELETE 
    public async Task<IActionResult> DeleteOnCategoryPage(Guid categoryId, Guid wordId)
    {
        var request = new RestRequest("api/Word/Delete", Method.Delete);
        request.AddQueryParameter("wordId", wordId);
        await _client.ExecuteAsync(request); 
        return RedirectToAction("List", "Word", new { categoryId });
    }

    public async Task<IActionResult> Delete(Guid wordId)
    {
        var request = new RestRequest("api/Word/Delete", Method.Delete);
        request.AddQueryParameter("wordId", wordId);
        await _client.ExecuteAsync(request); 
        return RedirectToAction("Index", "Word");
    }


    // UPDATE IMAGE
    public async Task<IActionResult> UpdateImage(Guid wordId)
    {
        var request = new RestRequest("api/Word/Get", Method.Get);
        request.AddQueryParameter("wordId", wordId);
        var response = await _client.GetAsync<WordResponseDto>(request);

        return View(response);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateImage(IFormFile file, Guid wordId, bool isFlow) // isFlow = true : when word created first time, false : edit image anytime 
    { 
        if (file == null) this.ModelState.AddModelError("file", "File is required!");
        if (wordId == default) this.ModelState.AddModelError("wordId", "An error occurred, please try again!"); 
        if (!this.ModelState.IsValid)  return RedirectToAction("UpdateImage", "Word", new { wordId });
      
        var request = new RestRequest("api/Word/ImageUpdate", Method.Post);
        request.AddHeader("Content-Type", "multipart/form-data");
        request.AddQueryParameter("wordId", wordId);
        using (var ms = new MemoryStream())
        {
            await file!.CopyToAsync(ms);
            request.AddFile("file", ms.ToArray(), file.FileName, file.ContentType);
        }
        var response = await _client.ExecuteAsync<WordResponseDto>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseDto = JsonConvert.DeserializeObject<WordResponseDto>(response.Content!);
            if (isFlow) // redirect to create another word for category
            {
                return RedirectToAction("Create", "Word", new { categoryId = responseDto!.CategoryId });
            }
            return RedirectToAction("List", "Word", new { categoryId = responseDto!.CategoryId });
        }

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return RedirectToAction("UpdateImage", "Word", new { wordId });
    }

    // DELETE IMAGE
    public async Task<IActionResult> DeleteImage(Guid wordId, string url)
    {
        if (wordId == default) this.ModelState.AddModelError(string.Empty, "An error occurred, please try again!");
        if (string.IsNullOrEmpty(url)) this.ModelState.AddModelError("url", "An error occurred, please try again!");
        if (!this.ModelState.IsValid) return RedirectToAction("UpdateImage", "Word", new { wordId });

        var request = new RestRequest("api/Word/ImageDelete", Method.Delete);
        request.AddQueryParameter("wordId", wordId);
        request.AddQueryParameter("url", url);
        var response = await _client.ExecuteAsync<WordResponseDto>(request);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return RedirectToAction("UpdateImage", "Word", new { wordId });

        ResponseHelper.HandleResponseError(response, this.ModelState);
        return RedirectToAction("UpdateImage", "Word", new { wordId });
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

    private async Task AddCategoryListToViewBag(Guid categoryId)
    {
        var request= new RestRequest("api/Category/GetAll", Method.Get);
        var categoryList = await _client.GetAsync<List<CategoryResponseDto>>(request);

        List<SelectListItem> categorySelectList = categoryList!
           .Select(c => new SelectListItem(text: c.English, c.Id.ToString(), selected: categoryId != Guid.Empty && c.Id == categoryId)).ToList();
        ViewBag.CategorySelectList = categorySelectList;
    }


    private FSPModel GenerateFSPModel(WordIndexFilterModel searchModel, int page, int size, string sortBy)
    {
        DynamicQuery dynamicQuery = new();
        List<Sort> sortList = new();
        Filter filter = new()
        {
            Field = "HasImage",
            Operator = "eq",
            Value = searchModel.OnlyHasntImage ? "false" : "true",
            Logic = "and",
            Filters = new()
        };

        if (searchModel.CategoryIdList != null && searchModel.CategoryIdList.Length > 0)
        {
            var categoryQuery = new Filter()
            {
                Field = "CategoryId",
                Operator = "eq",
                Value = searchModel.CategoryIdList[0].ToString(),
                Logic = "or",
                Filters = new()
            };
            for (int i = 1; i < searchModel.CategoryIdList.Length; i++)
            {
                categoryQuery.Filters.Add(new Filter()
                {
                    Field = "CategoryId",
                    Operator = "eq",
                    Value = searchModel.CategoryIdList[i].ToString()
                });
            }

            filter.Filters.Add(categoryQuery);
        }

        if (!string.IsNullOrEmpty(searchModel.WordSearch))
        {
            var wordContainsQuery = new Filter
            {
                Field = "English",
                Operator = "contains",
                Value = searchModel.WordSearch,
                Logic = "or",
                Filters =
                [
                    new Filter()
                    {
                        Field = "Turkish",
                        Operator = "contains",
                        Value = searchModel.WordSearch
                    }
                ]
            };

            filter.Filters.Add(wordContainsQuery);
        }

        switch (sortBy)
        {
            case "1":
                sortList.Add(new Sort() { Field = "English", Dir = "asc" });
                break;
            case "2":
                sortList.Add(new Sort() { Field = "English", Dir = "desc" });
                break;
            default:
                sortList.Add(new Sort() { Field = "English", Dir = "asc" });
                break;
        }


        dynamicQuery.Sort = sortList;
        dynamicQuery.Filter = filter;
        FSPModel fSPModel = new()
        {
            DynamicQuery = dynamicQuery,
            PagingRequest = new() { Page = page, PageSize = size }
        };
        return fSPModel;
    }
}
