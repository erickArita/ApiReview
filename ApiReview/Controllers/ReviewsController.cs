using System.Security.Claims;
using ApiReview.Common.Utils;
using ApiReview.Core.Autentication;
using ApiReview.Core.Reviews.Dtos;
using ApiReview.Domain;
using ApiReview.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

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
    private readonly IUserContextService _userContextService;

    public ReviewsController(AplicationDbContext context, IMapper mapper, IUserContextService userContextService)
    {
        _context = context;
        _mapper = mapper;
        _userContextService = userContextService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews(Guid bookId)
    {
        var existeLibro = await _context.Books.AnyAsync(x => x.Id == bookId);

        if (!existeLibro)
        {
            return NotFound(new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {bookId}"
            });
        }

        var recursiveReviews = await _context.Reviews
            .Where(r =>
                r.ParentReviewId == null) // Filtra por libro y solo revisiones principales
            .ToListAsync();


        var rewievsDto = _mapper.Map<List<ReviewDto>>(recursiveReviews);

        foreach (var respuestaDto in rewievsDto)
        {
            respuestaDto.Children = await GetRespuestasHijasRecursivo(respuestaDto.Id);
        }

        return Ok(new ResponseDto<List<ReviewDto>>
        {
            Status = true,
            Data = rewievsDto
        });
    }

    private async Task<ICollection<ReviewDto>> GetRespuestasHijasRecursivo(Guid respuestaId)
    {
        var respuestasDb = await _context.Reviews
            .Where(r => r.ParentReviewId == respuestaId)
            .ToListAsync();

        var respuestasDto = _mapper.Map<List<ReviewDto>>(respuestasDb);

        foreach (var respuestaDto in respuestasDto)
        {
            respuestaDto.Children = await GetRespuestasHijasRecursivo(respuestaDto.Id);
        }

        return respuestasDto;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> GetReview(Guid bookId, Guid id)

    {
        var existeLibro = await _context.Books.AnyAsync(x => x.Id == bookId);

        if (!existeLibro)
        {
            return NotFound(new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {bookId}"
            });
        }


        if (!ReviewExists(id))
        {
            return NotFound(new ResponseDto<ReviewDto>
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
        var userId = _userContextService.GetClaimsPrincipalAsync();

        var existeLibro = await _context.Books.AnyAsync(x => x.Id == bookId);

        if (!existeLibro)
        {
            return NotFound(new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {bookId}"
            });
        }

        var review = _mapper.Map<Review>(createReviewDto);
        review.CreatedAt = DateTime.Now;
        review.Id = Guid.NewGuid();
        review.UserId = userId.ToString();
        review.BookId = bookId;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var newReview = await _context.Reviews
            .Where(r => r.Id == review.Id)
            .Include(r => r.Respuestas)
            .ThenInclude(respuesta => respuesta.Respuestas)
            .FirstOrDefaultAsync();
        var reviewDto = _mapper.Map<ReviewDto>(newReview);

        return StatusCode(StatusCodes.Status201Created, new ResponseDto<ReviewDto>
        {
            Status = true,
            Message = "La review se creo correctamente",
            Data = reviewDto
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutReview(Guid bookId, Guid id, UpdateReviewDto updateReviewDto)
    {
        var existeLibro = await _context.Books.AnyAsync(x => x.Id == bookId);

        if (!existeLibro)
        {
            return NotFound(new ResponseDto<ReviewDto>
            {
                Status = false,
                Message = $"No existe el libro con el id: {bookId}"
            });
        }

        if (!ReviewExists(id))
        {
            return NotFound(new ResponseDto<ReviewDto>
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
            return NotFound(new ResponseDto<ReviewDto>
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