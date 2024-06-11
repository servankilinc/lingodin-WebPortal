using FluentValidation;

namespace WebApp.Models.Dtos.WordDtos;

public class FavoriteWordRequestDto : IDto
{
    public Guid UserId { get; set; }
    public Guid WordId { get; set; }
}


public class FavoriteWordRequestDtoValidator : AbstractValidator<FavoriteWordRequestDto>
{
    public FavoriteWordRequestDtoValidator()
    {
        RuleFor(x => x.UserId).NotNull();
        RuleFor(x => x.WordId).NotNull();
    }
}