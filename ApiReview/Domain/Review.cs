using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ApiReview.Domain;

public class Review
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string UserId { get; set; }
    public string Comment { get; set; }
    [Range(1, 5)] public int Rating { get; set; }
    public Guid? ParentReviewId { get; set; }
    
    [ForeignKey("ParentId")] public virtual Review? ParentReview { get; set; }
    [ForeignKey(nameof(UserId))] public virtual IdentityUser User { get; set; }
    public virtual Book Book { get; set; }
    public virtual List<Review> Respuestas { get; set; }
    public DateTime CreatedAt { get; set; }
}