using System.ComponentModel.DataAnnotations;

namespace Suwayami.DTO
{
    public class GenreCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
    }
}
