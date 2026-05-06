namespace Suwayami.DTO
{
    public class BookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public int PublishYear { get; set; }
        public AuthorDto? Author { get; set; }
        public GenreDto? Genre { get; set; }
    }
}
