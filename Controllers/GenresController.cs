using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using laboratory_4.Context;
using laboratory_4.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using Suwayami.DTO;

namespace Suwayami.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly LibraryContext _context;

        public GenresController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreDto>>> GetGenres()
        {
            var genres = await _context.Genres
                .Select(g => new GenreDto
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                })
                .ToListAsync();

            return Ok(genres);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenreDto>> GetGenre(int id)
        {
            var genre = await _context.Genres
                .Where(g => g.GenreId == id)
                .Select(g => new GenreDto
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                })
                .FirstOrDefaultAsync();

            if (genre == null)
                return NotFound();

            return Ok(genre);
        }

        [HttpPost]
        public async Task<ActionResult<GenreDto>> PostGenre(GenreCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var genre = new Genre
            {
                Name = dto.Name
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            var resultDto = new GenreDto
            {
                GenreId = genre.GenreId,
                Name = genre.Name
            };

            return CreatedAtAction(nameof(GetGenre), new { id = genre.GenreId }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutGenre(int id, GenreUpdateDto dto)
        {
            if (id != dto.GenreId)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            genre.Name = dto.Name;

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenreExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            // Проверка, есть ли книги с этим жанром
            if (genre.Books != null && genre.Books.Any())
                return BadRequest("Cannot delete genre with existing books.");

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GenreExists(int id)
        {
            return _context.Genres.Any(g => g.GenreId == id);
        }
    }
}