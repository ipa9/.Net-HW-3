namespace Reddit.Models;

public class TokenModel
{
    public string RefreshToken { get; set; }
    public string Email { get; internal set; }
}