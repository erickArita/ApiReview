namespace ApiReview.Services.GCS;

public interface ISigningService
{
    Task<string> SignAsync(string imageUrl, TimeSpan expiration);
    Task<string> SignAsync(string imageUrl);
}