namespace ApiReview.Core.Reviews.Dtos;

public record ReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public string UserId { get; set; }
    public Guid BookId { get; set; }
    public Guid? ParentReviewId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ReviewDto> Children { get; set; }
}