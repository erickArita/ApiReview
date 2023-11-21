using ApiReview.Common.Utils;
using ApiReview.Core.Autores.Dtos;
using ApiReview.Domain;
using ApiReview.Infrastructure.Persistence;
using ApiReview.Services.GCS;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiReview.Controllers;

[Authorize]
[Route("api/autores")]
[ApiController]
public class AutoresController : ControllerBase
{
    private readonly AplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly ISigningService _signingService;
    private static string _path = "autores";

    public AutoresController(
        AplicationDbContext context,
        IMapper mapper,
        IAlmacenadorArchivos almacenadorArchivos,
        ISigningService signingService)
    {
        _context = context;
        _mapper = mapper;
        _almacenadorArchivos = almacenadorArchivos;
        _signingService = signingService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AutorDto>>> Get()
    {
        var autoresDb = await _context.Autores.ToListAsync();
        var autoresDto = _mapper.Map<List<AutorDto>>(autoresDb);

        var autoresDtoSigned = await Task.WhenAll(autoresDto.Select(async autor =>
        {
            autor.Foto = await _signingService.SignAsync(autor.Foto);
            return autor;
        }));


        return Ok(new ResponseDto<IReadOnlyList<AutorDto>>
        {
            Status = true,
            Data = autoresDtoSigned
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseDto<AutorDto>>> GetOneById(Guid id)
    {
        var autorDb = await _context.Autores.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);

        if (autorDb is null)
        {
            return NotFound(new ResponseDto<AutorDto>
            {
                Status = false,
                Message = $"No existe el autor con el id: {id}"
            });
        }

        var autorDto = _mapper.Map<AutorGetByIdDto>(autorDb);

        autorDto.Foto = await _signingService.SignAsync(autorDto.Foto);

        return Ok(new ResponseDto<AutorDto>
        {
            Status = true,
            Data = autorDto
        });
    }

    [HttpPost]
    public async Task<ActionResult<ResponseDto<AutorDto>>> Post([FromForm] AutorCreateDto dto)
    {
        var autor = _mapper.Map<Autor>(dto);

        autor.Id = Guid.NewGuid();
        var link = await _almacenadorArchivos.GuardarArchivo(dto.Foto, $"${_path}/{autor.Id}");
        autor.Foto = link;
        _context.Add(autor);
        await _context.SaveChangesAsync();

        var autorDto = _mapper.Map<AutorDto>(autor);
        autorDto.Foto = await _signingService.SignAsync(autorDto.Foto);
        return StatusCode(StatusCodes.Status201Created, new ResponseDto<AutorDto>
        {
            Message = "El autor se creo con exito",
            Data = autorDto
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResponseDto<AutorDto>>> Put(Guid id, [FromForm] AutorUpdateDto dto)
    {
        var autorDb = await _context.Autores.FirstOrDefaultAsync(a => a.Id == id);
        if (autorDb is null)
        {
            return NotFound(new ResponseDto<AutorDto>
            {
                Status = false,
                Message = $"No existe el autor con el id: {id}"
            });
        }

       
        
        _mapper.Map<AutorUpdateDto, Autor>(dto, autorDb);
        if (dto?.Foto.Name is not null)
        {
            var link = await _almacenadorArchivos.EditarArchivo(dto.Foto, $"${_path}/{autorDb.Id}");
            autorDb.Foto = link;
        }
        _context.Update(autorDb);

        await _context.SaveChangesAsync();

        var autorDto = _mapper.Map<AutorDto>(autorDb);
        autorDto.Foto = await _signingService.SignAsync(autorDto.Foto);
        return Ok(new ResponseDto<AutorDto>
        {
            Status = true,
            Message = "El autor se actualizo con exito",
            Data = autorDto
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ResponseDto<string>>> Delete(Guid id)
    {
        var autor = await _context.Autores.FirstOrDefaultAsync(a => a.Id == id);
        if (autor is null)
        {
            return NotFound(new ResponseDto<string>
            {
                Status = false,
                Message = $"No existe el autor con el id: {id}"
            });
        }

        _context.Remove(autor);
        await _context.SaveChangesAsync();
        await _almacenadorArchivos.BorrarArchivo($"${_path}/{autor.Id}");
        return Ok(new ResponseDto<string>
        {
            Status = true,
            Message = "El autor se elimino con exito"
        });
    }
}