using Google.Cloud.Storage.V1;
using Microsoft.IdentityModel.Tokens;

namespace ApiReview.Services.GCS;

public class AlmacenadorArchivosGoogleCloud : IAlmacenadorArchivos
{
    private readonly GCPConfig _gcpConfig;

    public AlmacenadorArchivosGoogleCloud(IConfiguration configuration)
    {
        _gcpConfig = configuration.GetSection("GCPConfig").Get<GCPConfig>() ??
                     throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<string> GuardarArchivo(IFormFile contenido, string contenedor)
    {
        if (contenido.FileName.IsNullOrEmpty()) return string.Empty;
        var contenidoArchivo = ReadFully(contenido);
        var client = StorageClient.Create();
        var extension = Path.GetExtension(contenido.FileName);
        var nombreArchivo = $"{contenedor}/{Guid.NewGuid()}";

        var obj = await client.UploadObjectAsync(_gcpConfig.BucketName, nombreArchivo, extension, contenidoArchivo);
        return obj.Name;
    }

    private static Stream ReadFully(IFormFile input)
    {
        var ms = new MemoryStream();
        input.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }


    public async Task BorrarArchivo(string ruta)
    {
        var client = StorageClient.Create();
        await client.DeleteObjectAsync(_gcpConfig.BucketName, ruta);
    }

    public async Task<string> EditarArchivo(IFormFile contenido, string objName)
    {
        if (contenido.FileName.IsNullOrEmpty()) return string.Empty;
        var contenidoArchivo = ReadFully(contenido);
        var client = StorageClient.Create();
        var extension = Path.GetExtension(contenido.FileName);
        var obj = await client.UploadObjectAsync(_gcpConfig.BucketName, objName, extension, contenidoArchivo);
        
        return obj.Name;
    }
}