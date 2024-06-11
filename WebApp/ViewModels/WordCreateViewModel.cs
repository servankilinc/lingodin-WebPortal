using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models.Dtos.CategoryDtos;
using WebApp.Models.Dtos.WordDtos;

namespace WebApp.ViewModels;

public class WordCreateViewModel
{
    public CategoryResponseDto? Category { get; set; }
    public WordCreateDto? WordCreateDto { get; set; }

    public WordCreateViewModel(CategoryResponseDto? category)
    {
        Category = category;
    }
    public WordCreateViewModel()
    {
    }
}

public class WordUpdateViewModel
{
    public WordUpdateDto? WordUpdateDto { get; set; }
    public List<SelectListItem>? Categories { get; set; }

    public WordUpdateViewModel(WordUpdateDto? word, List<SelectListItem> categories)
    {
        WordUpdateDto = word;
        Categories = categories;
    }
    public WordUpdateViewModel()
    {
    }
}
