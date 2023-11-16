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

[Route("api/[controller]")]
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
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
    {
        var reviews = await _context.Reviews.ToListAsync();
        var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);
        return Ok(reviewDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(Guid id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
        {
            return NotFound();
        }

        var reviewDto = _mapper.Map<ReviewDto>(review);
        return Ok(reviewDto);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> PostReview(CreateReviewDto createReviewDto)
    {
        var review = _mapper.Map<Review>(createReviewDto);
        review.CreatedAt = DateTime.Now;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var reviewDto = _mapper.Map<ReviewDto>(review);
        return CreatedAtAction("GetReview", new { id = reviewDto.Id }, reviewDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutReview(Guid id, UpdateReviewDto updateReviewDto)
    {
        if (id != updateReviewDto.Id)
        {
            return BadRequest();
        }

        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        _mapper.Map(updateReviewDto, review);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReviewExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ReviewExists(Guid id)
    {
        return _context.Reviews.Any(e => e.Id == id);
    }
}