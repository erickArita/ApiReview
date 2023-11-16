using Microsoft.Build.Framework;

namespace ApiReview.Core.Autentication.Models;

public class LoginUser
{
    [Required] public string Username { get; set; }
    [Required] public string Password { get; set; }
}