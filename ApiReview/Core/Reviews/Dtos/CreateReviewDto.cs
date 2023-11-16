using Microsoft.Build.Framework;

namespace ApiReview.Core.Reviews.Dtos;

public record CreateReviewDto
{
    public int Calificacion { get; set; }
    public string Comentario { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid LibroId { get; set; }
    public Guid? ParentId { get; set; }
}