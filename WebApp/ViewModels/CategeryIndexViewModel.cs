using WebApp.Models.Dtos.CategoryDtos;

namespace WebApp.ViewModels;

public class CategeryIndexViewModel
{
    public List<CategoryResponseDto>? CategoryList { get; set; }
    public CategoryImageUpdateDto? ImageUpdate { get; set; }
}