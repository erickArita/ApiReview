namespace ApiReview.Services;

public class AlmacenadorArchivosGoogleCloud : IAlmacenadorArchivos
{
    
    
    public AlmacenadorArchivosGoogleCloud(IConfiguration configuration)
    {
        
    }
    public Task<string> GuardarArchivo(byte[] contenido, string extension, string contenedor, string contentType)
    {
        throw new NotImplementedException();
    }

    public Task<string> EditarArchivo(byte[] contenido, string extension, string contenedor, string ruta, string contentType)
    {
        throw new NotImplementedException();
    }

    public Task BorrarArchivo(string ruta, string contenedor)
    {
        throw new NotImplementedException();
    }
}