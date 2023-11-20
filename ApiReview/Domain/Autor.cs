using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ApiReview.Domain;

[Table("autores", Schema = "transacctional")]
public class Autor
{
    [Column("id")]  
    public Guid Id { get; set; }
    [Column("name")]
    [Required]
    [StringLength(70)]
    public string Name { get; set; }

    public virtual IEnumerable<Book> Books { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string Foto { get; set; }
}