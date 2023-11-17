using ApiReview.Common.Utils;
using ApiReview.Core.Books.Dtos;
using ApiReview.Domain;
using ApiReview.Infrastructure.Persistence;
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

    public BooksController(AplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ResponseDto<IReadOnlyList<BookDto>>>> Get()
    {
        var booksDb = await _context.Books.Include(b => b.Autor).ToListAsync();

        var booksDto = _mapper.Map<List<BookDto>>(booksDb);

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

        return Ok(new ResponseDto<BookDto>
        {
            Status = true,
            Data = bookDto
        });
    }

    [HttpPost]
    public async Task<ActionResult<ResponseDto<BookDto>>> Post(BookCreateDto dto) //crear un libro
    {
        /*if (!ModelState.IsValid)
        {
            return BadRequest("Los datos del libro son incorrectos");
        }*/ //ya no porque tenemos un middleware que lo hace por nosotros

        var autorExiste = await _context.Autores.AnyAsync(x => x.Id == dto.AutorId);
        if (!autorExiste)
        {
            return NotFound(new ResponseDto<BookDto>
            {
                Status = false,
                Message = $"No existe el autor: {dto.AutorId}" // el $ para inyectar el valor de la variable
            });
        }

        var book = _mapper.Map<Book>(dto);

        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
        var newBook = await _context.Books
            .Include(b => b.Autor)
            .FirstOrDefaultAsync(x => x.Id == book.Id);
        var bookDto = _mapper.Map<BookDto>(newBook);

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

        _context.Update(bookDb);
        await _context.SaveChangesAsync();
        
        var book = await _context.Books
            .Include(b => b.Autor)
            .FirstOrDefaultAsync(x => x.Id == bookDb.Id);

        var bookDto = _mapper.Map<BookDto>(book);

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

        return Ok(new ResponseDto<string>
        {
            Status = true,
            Message = "El libro se elimino correctamente"
        });
    }
}