namespace ApiReview.Services.GCS;

public interface IAlmacenadorArchivos
{
    Task<string> GuardarArchivo(IFormFile contenido, string contenedor);
    Task BorrarArchivo(string ruta);
    Task<string> EditarArchivo(IFormFile contenido, string contenedor);
}