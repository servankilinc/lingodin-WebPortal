namespace WebApp.Models.Auth;

public class AccessToken
{
    public string? Token { get; set; }
    public DateTime Expiration { get; set; }
}