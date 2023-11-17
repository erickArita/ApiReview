using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiReview.Common.Utils;
using ApiReview.Core.Autentication.Models;
using ArcadeMachine.Core.Autentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ApiReview.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthenticationController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("Register")]
    public async Task<ActionResult<ResponseDto<object>>> Register([FromBody] RegisterUser registerUser)
    {
        // Check if the user exists
        var existUser = await _userManager.FindByEmailAsync(registerUser.Email);
        var existUserName = await _userManager.FindByNameAsync(registerUser.Username);

        if (existUserName != null)
        {
            return BadRequest(new ResponseDto<object>
            {
                Status = false,
                Message = $"El usuario con nombre {registerUser.Username} ya existe"
            });
        }

        // Create the user
        var user = new IdentityUser
        {
            UserName = registerUser.Username,
            Email = registerUser.Email
        };

        var result = await _userManager.CreateAsync(user, registerUser.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new ResponseDto<object>
            {
                Status = false,
                Message = result.Errors.Select(x => x.Description).FirstOrDefault()
            });
        }

        var login = await Login(new LoginUser
        {
            Username = registerUser.Username,
            Password = registerUser.Password
        });


        return Ok(login);
    }

    [HttpPost]
    [Route("Login")]
    public async Task<ResponseDto<object>> Login([FromBody] LoginUser loginUser)
    {
        var result = await _signInManager.PasswordSignInAsync(loginUser.Username, loginUser.Password, false, false);
        if (result.Succeeded)
        {
            var user = (await _userManager.FindByNameAsync(loginUser.Username));

            var authClaims = new List<Claim>()
            {
                new(ClaimTypes.Name, loginUser.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id)
            };

            var jwtToken = GetToken(authClaims);

            return new ResponseDto<object>
            {
                Status = true,
                Message = $"{user.UserName} registrado correctamente",
                Data = new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo.Millisecond,
                    user = user.UserName,
                    userId = user.Id
                }
            };
        }
        else
        {
            throw new Exception();
        }
    }

    private JwtSecurityToken GetToken(List<Claim> claims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return token;
    }
}