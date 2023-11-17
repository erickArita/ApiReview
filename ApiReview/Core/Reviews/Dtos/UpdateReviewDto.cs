namespace ApiReview.Core.Reviews.Dtos;

public record UpdateReviewDto
{
    public int Rating { get; set; }
    public string Comment { get; set; }
}