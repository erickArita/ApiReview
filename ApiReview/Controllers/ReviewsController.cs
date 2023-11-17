using ApiReview.Common.Utils;
using ApiReview.Core.Reviews.Dtos;
using ApiReview.Domain;
using ApiReview.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace ApiReview.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/books/{bookId:guid}/reviews")]
[ApiController]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly AplicationDbContext _context;
    private readonly IMapper _mapper;

    public ReviewsController(AplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews( Guid bookId)
    {
        var existeLibro = await _context.Books.AnyAsync(x => x.Id == bookId);
        
        if (!existeLibro)
        {
            return NotFound( new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {bookId}"
            });
        }
        var reviews = await _context.Reviews
            .Where(r => r.BookId == bookId && r.ParentReviewId == null)
            .Include(r => r.Respuestas) // Cargar respuestas de manera ansiosa
            .ThenInclude(respuesta => respuesta.Respuestas) // Cargar respuestas de respuestas de manera ansiosa
            .ToListAsync();


        
        var rewievsDto = _mapper.Map<List<ReviewDto>>(reviews);

        return Ok(new ResponseDto<List<ReviewDto>>
        {
            Status = true,
            Data = rewievsDto
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> GetReview(Guid bookId, Guid id)
    
    {
        var existeLibro = await _context.Books.AnyAsync(x => x.Id == bookId);
        
        if (!existeLibro)
        {
            return NotFound( new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {bookId}"
            });
        }
       

        if (!ReviewExists(id))
        {
            return NotFound( new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe la review con el id: {id}"
            });
        }
        var review = await _context.Reviews.FirstOrDefaultAsync(reviewDB => reviewDB.Id == id);
        var reviewDto = _mapper.Map<ReviewDto>(review);
        return Ok(reviewDto);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> PostReview(Guid bookId, CreateReviewDto createReviewDto)
    {
        var existeLibro = await _context.Books.AnyAsync(x => x.Id == bookId);
        
        if (!existeLibro)
        {
            return NotFound( new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {bookId}"
            });
        }
        var review = _mapper.Map<Review>(createReviewDto);
        review.CreatedAt = DateTime.Now;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var reviewDto = _mapper.Map<ReviewDto>(review);
        
        return StatusCode(StatusCodes.Status201Created, new ResponseDto<ReviewDto>
        {
            Status = true,
            Message = "La review se creo correctamente",
            Data = reviewDto //_mapper.Map<BookDto>(book)
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutReview(Guid bookId, Guid id, UpdateReviewDto updateReviewDto)
    {
        var existeLibro = await _context.Books.AnyAsync(x => x.Id == bookId);
        
        if (!existeLibro)
        {
            return NotFound( new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {bookId}"
            });
        }
        if (!ReviewExists(id))
        {
            return NotFound( new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe la review con el id: {id}"
            });
        }

        var review = await _context.Reviews.FindAsync(id);
        _mapper.Map(updateReviewDto, review);

     

        return Ok(new ResponseDto<ReviewDto>
        {
            Status = true,
            Message = "La review se actualizo correctamente"
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        
        if (!ReviewExists(id))
        {
            return NotFound( new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe la review con el id: {id}"
            });
        }

        var review = await _context.Reviews.FindAsync(id);
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(new ResponseDto<ReviewDto>
        {
            Status = true,
            Message = "La review se borro correctamente"
        });
    }

    private bool ReviewExists(Guid id)
    {
        return _context.Reviews.Any(e => e.Id == id);
    }

  
}