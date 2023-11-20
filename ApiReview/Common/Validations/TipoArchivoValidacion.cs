using System.ComponentModel.DataAnnotations;

namespace ApiReview.Common.Validations;

public class TipoArchivoValidacion : ValidationAttribute
{
    private readonly string[] tiposValidos;

    public TipoArchivoValidacion(string[] tiposValidos)
    {
        this.tiposValidos = tiposValidos;
    }

    public TipoArchivoValidacion(GrupoTipoArchivo grupoTipoArchivo)
    {
        if (grupoTipoArchivo == GrupoTipoArchivo.Imagen)
        {
            tiposValidos = new string[] { "image/jpeg", "image/png", "image/gif" };
        }
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }
        IFormFile formFile = value as IFormFile;

        if (formFile is null)
        {
            return ValidationResult.Success;
        }

        if (!tiposValidos.Contains(formFile.ContentType))
        {
            return new ValidationResult($"el tipo de los archivos debe ser: {string.Join(", ", tiposValidos)}");
        }
        return ValidationResult.Success;
    }
}