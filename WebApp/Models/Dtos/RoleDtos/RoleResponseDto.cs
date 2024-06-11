namespace WebApp.Models.Dtos.RoleDtos;

public class RoleResponseDto : IDto
{
    public Guid Id { get; set; } 
    public string? Name { get; set; }
}