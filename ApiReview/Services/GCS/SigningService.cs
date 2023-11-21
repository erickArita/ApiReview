using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;

namespace ApiReview.Services.GCS;

public class SigningService : ISigningService
{
    private string _bucketName;
    private UrlSigner _signer;

    public SigningService(IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        var sac = GoogleCredential.FromFile(env);
        _signer = UrlSigner.FromCredential(sac);
        _bucketName = configuration.GetSection("GCPConfig").Get<GCPConfig>().BucketName ??
                      throw new ArgumentNullException(nameof(configuration));
    }

    private async Task<string> SignAsyncBase(string imageUrl, TimeSpan expiration)
    {
        if (string.IsNullOrEmpty(imageUrl)) return string.Empty;
        string signedUrl = await _signer.SignAsync(_bucketName, imageUrl, expiration, HttpMethod.Get);
        return signedUrl;
    }

    public Task<string> SignAsync(string imageUrl, TimeSpan expiration)
    {
        return SignAsyncBase(imageUrl, expiration);
    }

    public async Task<string> SignAsync(string imageUrl)
    {
        var defaultExpiration = TimeSpan.FromMinutes(60);
        return await SignAsyncBase(imageUrl, defaultExpiration);
    }
}