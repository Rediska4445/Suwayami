using Microsoft.AspNetCore.Mvc;
using laboratory_4.Context;
using laboratory_4.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using Suwayami.DTO;

namespace laboratory_4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet("byauthor")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByAuthor(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required");

            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Where(b => b.Author.FullName.Contains(name))
                .OrderByDescending(b => b.PublishYear)
                .ThenBy(b => b.Title)
                .Select(b => new BookDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    PublishYear = b.PublishYear,
                    Author = new AuthorDto
                    {
                        AuthorId = b.Author.AuthorId,
                        FullName = b.Author.FullName
                    },
                    Genre = new GenreDto
                    {
                        GenreId = b.Genre.GenreId,
                        Name = b.Genre.Name
                    }
                })
                .ToListAsync();

            return Ok(books);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Select(b => new BookDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    PublishYear = b.PublishYear,
                    Author = new AuthorDto
                    {
                        AuthorId = b.Author.AuthorId,
                        FullName = b.Author.FullName
                    },
                    Genre = new GenreDto
                    {
                        GenreId = b.Genre.GenreId,
                        Name = b.Genre.Name
                    }
                })
                .ToListAsync();

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Where(b => b.BookId == id)
                .Select(b => new BookDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    PublishYear = b.PublishYear,
                    Author = new AuthorDto
                    {
                        AuthorId = b.Author.AuthorId,
                        FullName = b.Author.FullName
                    },
                    Genre = new GenreDto
                    {
                        GenreId = b.Genre.GenreId,
                        Name = b.Genre.Name
                    }
                })
                .FirstOrDefaultAsync();

            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> PostBook(BookCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = new Book
            {
                Title = dto.Title,
                PublishYear = dto.PublishYear,
                AuthorId = dto.AuthorId,
                GenreId = dto.GenreId,
                Author = _context.Authors.Find(dto.AuthorId),
                Genre = _context.Genres.Find(dto.GenreId)
            };

            if (book.Author == null || book.Genre == null)
                return BadRequest("Author or Genre not found.");

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var resultDto = new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                PublishYear = book.PublishYear,
                Author = new AuthorDto
                {
                    AuthorId = book.Author.AuthorId,
                    FullName = book.Author.FullName
                },
                Genre = new GenreDto
                {
                    GenreId = book.Genre.GenreId,
                    Name = book.Genre.Name
                }
            };

            return CreatedAtAction(nameof(GetBook), new { id = book.BookId }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, BookUpdateDto dto)
        {
            if (id != dto.BookId)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            var author = await _context.Authors.FindAsync(dto.AuthorId);
            if (author == null)
                return BadRequest("Author not found.");

            var genre = await _context.Genres.FindAsync(dto.GenreId);
            if (genre == null)
                return BadRequest("Genre not found.");

            book.Title = dto.Title;
            book.PublishYear = dto.PublishYear;
            book.AuthorId = dto.AuthorId;
            book.GenreId = dto.GenreId;

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(b => b.BookId == id);
        }
    }
}