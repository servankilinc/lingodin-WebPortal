using WebApp.Models.Dtos.RoleDtos;

namespace WebApp.Models.Dtos.UserDtos;

public class UserDetailResponseDto : IDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public List<RoleResponseDto>? Roles { get; set; }
}