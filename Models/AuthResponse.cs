﻿namespace Reddit.Models;

public class AuthResponse
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }

    public DateTime TokenExpiryTime { set; get; }

}