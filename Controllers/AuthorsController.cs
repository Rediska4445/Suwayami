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
    public class AuthorsController : ControllerBase
    {
        private readonly LibraryContext _context;

        public AuthorsController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
        {
            var authors = await _context.Authors
                .Select(a => new AuthorDto
                {
                    AuthorId = a.AuthorId,
                    FullName = a.FullName
                })
                .ToListAsync();

            return Ok(authors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorDto>> GetAuthor(int id)
        {
            var author = await _context.Authors
                .Where(a => a.AuthorId == id)
                .Select(a => new AuthorDto
                {
                    AuthorId = a.AuthorId,
                    FullName = a.FullName
                })
                .FirstOrDefaultAsync();

            if (author == null)
                return NotFound();

            return Ok(author);
        }

        [HttpPost]
        public async Task<ActionResult<AuthorDto>> PostAuthor(AuthorCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var author = new Author
            {
                FullName = dto.FullName
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var resultDto = new AuthorDto
            {
                AuthorId = author.AuthorId,
                FullName = author.FullName
            };

            return CreatedAtAction(nameof(GetAuthor), new { id = author.AuthorId }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuthor(int id, AuthorUpdateDto dto)
        {
            if (id != dto.AuthorId)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            author.FullName = dto.FullName;

            _context.Entry(author).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            if (author.Books != null && author.Books.Any())
            {
                return BadRequest("Cannot delete author with existing books.");
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(a => a.AuthorId == id);
        }
    }
}