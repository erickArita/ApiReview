using ApiReview.Common.Utils;
using ApiReview.Core.Books.Dtos;
using ApiReview.Domain;
using ApiReview.Infrastructure.Persistence;
using ApiReview.Services.GCS;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiReview.Controllers;

[Route("api/books")]
[ApiController]
[Authorize] //para que pida autenticacion
public class BooksController : ControllerBase
{
    private readonly AplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly ISigningService _signingService;
    private readonly string _path = "books";

    public BooksController(AplicationDbContext context,
        IMapper mapper,
        IAlmacenadorArchivos almacenadorArchivos,
        ISigningService signingService
    )
    {
        _context = context;
        _mapper = mapper;
        _almacenadorArchivos = almacenadorArchivos;
        _signingService = signingService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ResponseDto<IReadOnlyList<BookDto>>>> Get()
    {
        var booksDb = await _context.Books.Include(b => b.Autor).ToListAsync();

        var booksDto = _mapper.Map<List<BookDto>>(booksDb);

        var booksDtoSigned = await Task.WhenAll(booksDto.Select(async book =>
        {
            book.Portada = await _signingService.SignAsync(book.Portada);
            return book;
        }));

        return Ok(new ResponseDto<IReadOnlyList<BookDto>>
        {
            Status = true,
            Data = booksDto
        });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous] // para que no pida autenticacion
    public async Task<ActionResult<ResponseDto<BookDto>>> GetById(Guid id)
    {
        var bookDb = await _context.Books
            .Include(b => b.Autor) //para que incluya el autor y se mire en la peticion es como el join de sql
            .FirstOrDefaultAsync(x => x.Id == id);
        if (bookDb is null)
        {
            return NotFound(new ResponseDto<BookDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {id}"
            });
        }

        var bookDto = _mapper.Map<BookDto>(bookDb);
        bookDto.Portada = await _signingService.SignAsync(bookDto.Portada);
        return Ok(new ResponseDto<BookDto>
        {
            Status = true,
            Data = bookDto
        });
    }

    [HttpPost]
    public async Task<ActionResult<ResponseDto<BookDto>>> Post(BookCreateDto dto) //crear un libro
    {
        var autorExiste = await _context.Autores.AnyAsync(x => x.Id == dto.AutorId);
        if (!autorExiste)
        {
            return NotFound(new ResponseDto<BookDto>
            {
                Status = false,
                Message = $"No existe el autor: {dto.AutorId}" // el $ para inyectar el valor de la variable
            });
        }

        var id = Guid.NewGuid();
        var portada = await _almacenadorArchivos.GuardarArchivo(dto.Portada, $"{_path}/{id}");
        var book = _mapper.Map<Book>(dto);
        book.Id = id;
        book.Portada = portada;

        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();

        var newBook = await _context.Books
            .Include(b => b.Autor)
            .FirstOrDefaultAsync(x => x.Id == book.Id);

        var bookDto = _mapper.Map<BookDto>(newBook);

        bookDto.Portada = await _signingService.SignAsync(bookDto.Portada);

        return StatusCode(StatusCodes.Status201Created, new ResponseDto<BookDto>
        {
            Status = true,
            Message = "El libro se creo correctamente",
            Data = bookDto
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ResponseDto<BookDto>>> Put(Guid id, BookUpdateDto dto) //actualizar un libro
    {
        var bookDb = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);
        if (bookDb is null)
        {
            return NotFound(new ResponseDto<BookDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {id}"
            });
        }

        var autorExiste = await _context.Autores.AnyAsync(x => x.Id == dto.AutorId);
        if (!autorExiste)
        {
            return NotFound(new ResponseDto<BookDto>
            {
                Status = false,
                Message = $"No existe el autor: {dto.AutorId}" // el $ para inyectar el valor de la variable
            });
        }

        _mapper.Map<BookUpdateDto, Book>(dto, bookDb);
      if (dto.Portada != null)
        {
            var portada = await _almacenadorArchivos.EditarArchivo(dto.Portada, $"{_path}/{id}");
            bookDb.Portada = portada;
        }
        _context.Update(bookDb);
        await _context.SaveChangesAsync();

        var book = await _context.Books
            .Include(b => b.Autor)
            .FirstOrDefaultAsync(x => x.Id == bookDb.Id);

        var bookDto = _mapper.Map<BookDto>(book);
        var bookDtoSigned = await _signingService.SignAsync(bookDto.Portada);
        bookDto.Portada = bookDtoSigned;
        return Ok(new ResponseDto<BookDto>
        {
            Status = true,
            Message = "El libro se actualizo correctamente",
            Data = bookDto //_mapper.Map<BookDto>(book)
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ResponseDto<string>>> Delete(Guid id)
    {
        var bookExist = await _context.Books.AnyAsync(x => x.Id == id);
        if (!bookExist)
        {
            return NotFound(new ResponseDto<BookDto>
            {
                Status = false,
                Message = $"No existe el libro: {id}"
            });
        }

        _context.Remove(new Book() { Id = id });
        await _context.SaveChangesAsync();
        await _almacenadorArchivos.BorrarArchivo($"{_path}/{id}");

        return Ok(new ResponseDto<string>
        {
            Status = true,
            Message = "El libro se elimino correctamente"
        });
    }
}