using System.Security.Claims;

namespace ApiReview.Core.Autentication;

public interface IUserContextService
{
    Guid GetClaimsPrincipalAsync();
}