using WebApp.Models.Dtos.WordDtos;

namespace WebApp.Models.ViewModels;

public class WordByUserModel
{
    public WordResponseDto? Word { get; set; }
    public bool IsWordAddedFav { get; set; }
}