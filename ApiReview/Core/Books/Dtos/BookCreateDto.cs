using System.ComponentModel.DataAnnotations;

namespace ApiReview.Core.Books.Dtos;

public class BookCreateDto
{
    //solo las propiedades que queremos que nos aparezcan en el formulario
    [Display(Name = "ISBN")]
    [StringLength(50, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres")]
    [Required(ErrorMessage = "El campo {0} es requerido")]
    public string ISBN { get; set; }
    [Display(Name = "Título")]
    [StringLength(50, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres")]
    [Required(ErrorMessage = "El campo {0} es requerido")]
    public string Title { get; set; }
    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
    [Display(Name = "Autor")]
    [Required(ErrorMessage = "El campo {0} es requerido")]
    public Guid AutorId { get; set; }
}