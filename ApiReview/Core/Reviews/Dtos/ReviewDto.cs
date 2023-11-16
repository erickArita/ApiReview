namespace ApiReview.Core.Reviews.Dtos;

public record ReviewDto
(
    Guid Id,
    int Calificacion,
    string Comentario,
    string UsuarioId,
    Guid LibroId,
    Guid? ParentId,
    DateTime FechaCreacion
);