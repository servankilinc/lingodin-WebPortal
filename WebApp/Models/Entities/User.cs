using WebApp.Models.Auth;

namespace WebApp.Models.Entities;

public class User : IEntity
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public byte[]? PasswordHash { get; set; }
    public AutheticatorType AutheticatorType { get; set; }

    public ICollection<Favorite>? Favorites { get; set; }
    public ICollection<Learned>? Learneds { get; set; }
    public ICollection<UserRoles>? UserRoles { get; set; }
}