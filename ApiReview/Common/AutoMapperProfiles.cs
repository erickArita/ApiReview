using ApiReview.Core.Autores.Dtos;
using ApiReview.Core.Books.Dtos;
using ApiReview.Core.Reviews.Dtos;
using ApiReview.Domain;
using AutoMapper;

namespace ApiReview.Common;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        MapsForBooks();
        MapsForAutores();
        MapsForReviews();
    }

    private void MapsForBooks()
    {
        //CreateMap<BookDto, Book>().ReverseMap();
        CreateMap<Book, BookDto>().ForPath(dest => dest.AutorNombre, opt => opt.MapFrom(src => src.Autor.Name));

        CreateMap<BookCreateDto, Book>();
    }

    private void MapsForAutores()
    {
        CreateMap<Autor, AutorDto>();
        CreateMap<AutorCreateDto, Autor>();
        CreateMap<Autor, AutorCreateDto>();
        CreateMap<Autor, AutorGetByIdDto>();
    }

    private void MapsForReviews()
    {
        CreateMap<Review, ReviewDto>();
        CreateMap<CreateReviewDto, Review>();
        CreateMap<UpdateReviewDto, Review>();
    }
}