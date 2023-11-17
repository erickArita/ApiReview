using Microsoft.IdentityModel.JsonWebTokens;

namespace ApiReview.Core.Autentication;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetClaimsPrincipalAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated ?? false)
        {
            // Usuario autenticado, devolver el principal de claims
            return Guid.Parse(
                httpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value ??
                Guid.Empty.ToString());
        }

        // No autenticado, puedes manejar esto según tus necesidades (por ejemplo, redirigir a la página de inicio de sesión)
        return Guid.Empty;
    }
}