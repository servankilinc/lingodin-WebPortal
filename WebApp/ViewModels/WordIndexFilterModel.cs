namespace WebApp.ViewModels;

public class WordIndexFilterModel
{
    public Guid[]? CategoryIdList { get; set; }
    public string? WordSearch { get; set; }
    public bool OnlyHasntImage { get; set; }
}