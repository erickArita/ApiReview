using System.ComponentModel.DataAnnotations;

namespace ApiReview.Common.Validations;

public class PesoArchivoValidacion: ValidationAttribute
{
    private readonly int pesoMaximoEnMegaBytes;
    public PesoArchivoValidacion(int PesoMaximoEnMegaBytes)
    {
        pesoMaximoEnMegaBytes = PesoMaximoEnMegaBytes;
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

        if (formFile.Length > pesoMaximoEnMegaBytes * 1024 * 1024)
        {
            return new ValidationResult($"El peso de la imagen no debe ser mayor a {pesoMaximoEnMegaBytes}MB");
        }
        {
            
        }

        return ValidationResult.Success;
    }
}