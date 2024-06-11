using WebApp.Models.Auth;
using WebApp.Models.Dtos.RoleDtos;

namespace WebApp.Models.ViewModels;

public class AccessTokenResultModel
{
    public AccessToken? AccessToken { get; set; }
    public ICollection<RoleResponseDto>? Roles { get; set; }
}