using System.ComponentModel.DataAnnotations;
using ApiReview.Common.Validations;

namespace ApiReview.Core.Autores.Dtos;

public class AutorCreateDto
{
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es requerido")]
    [StringLength(70, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres")]
    public string Name { get; set; }
    [Required(ErrorMessage = "El campo {0} es requerido")]
    [PesoArchivoValidacion(3)]
    [TipoArchivoValidacion(grupoTipoArchivo: GrupoTipoArchivo.Imagen)]
    public IFormFile Foto { get; set; }
}