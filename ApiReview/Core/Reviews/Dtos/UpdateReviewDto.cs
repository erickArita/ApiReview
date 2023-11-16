namespace ApiReview.Core.Reviews.Dtos;

public record UpdateReviewDto
{
    public Guid Id { get; set; }
    public int Calificacion { get; set; }
    public string Comentario { get; set; }
}