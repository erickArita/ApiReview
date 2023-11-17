using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ApiReview.Core.Autentication;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager;

    public UserContextService(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public Guid GetClaimsPrincipalAsync()
    {
        var usuarioActual = _httpContextAccessor.HttpContext.User;
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated ?? false)
        {
            var usuario = _userManager.GetUserId(usuarioActual);
            // Usuario autenticado, devolver el principal de claims
            return Guid.Parse(usuario);
        }

        // No autenticado, puedes manejar esto según tus necesidades (por ejemplo, redirigir a la página de inicio de sesión)
        return Guid.Empty;
    }
}