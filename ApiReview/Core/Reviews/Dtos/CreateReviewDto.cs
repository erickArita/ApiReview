using Microsoft.Build.Framework;

namespace ApiReview.Core.Reviews.Dtos;

public record CreateReviewDto
{
    public int Calificacion { get; set; }
    public string Comment { get; set; }
    public Guid? ParentReviewId { get; set; }
}