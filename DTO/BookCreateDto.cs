namespace Suwayami.DTO
{
    public class BookCreateDto
    {
        public string Title { get; set; } = null!;
        public int PublishYear { get; set; }
        public int AuthorId { get; set; }
        public int GenreId { get; set; }
    }
}
