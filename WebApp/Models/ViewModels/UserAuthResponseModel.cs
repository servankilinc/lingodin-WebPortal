using WebApp.Models.Auth;
using WebApp.Models.Dtos.RoleDtos;
using WebApp.Models.Dtos.UserDtos;

namespace WebApp.Models.ViewModels;

public class UserAuthResponseModel : IDto
{
    public UserResponseDto? User { get; set; }
    public AccessToken? AccessToken { get; set; }
    public ICollection<RoleResponseDto>? Roles { get; set; }
}