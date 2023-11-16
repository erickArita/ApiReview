using ApiReview.Core.Books.Dtos;

namespace ApiReview.Core.Autores.Dtos;

public class AutorGetByIdDto : AutorDto
{
    public IEnumerable<BookDto> Books { get; set; }
}