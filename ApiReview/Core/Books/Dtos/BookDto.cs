namespace ApiReview.Core.Books.Dtos;

public class BookDto
{
    public Guid Id { get; set; }
    public string ISBN { get; set; }
    public string Title { get; set; }
    public DateTime PublicationDate { get; set; }
    public Guid AutorId { get; set; }
    public string AutorName { get; set; }
}