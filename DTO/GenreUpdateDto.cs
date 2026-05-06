using System.ComponentModel.DataAnnotations;

namespace Suwayami.DTO
{
    public class GenreUpdateDto
    {
        public int GenreId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
    }
}
