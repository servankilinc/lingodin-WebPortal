using WebApp.Models.Dtos.CategoryDtos;

namespace WebApp.Models.ViewModels;

public class CategoryByUserModel
{
    public CategoryResponseDto? Category { get; set; }
    public int LearnedWordCount { get; set; } = 0;
    public int TotalWordCount { get; set; } = 0;
}