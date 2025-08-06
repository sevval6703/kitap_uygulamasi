using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using EBookAPI.Data;
using EBookAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly EBookDbContext _context;

        public FavoritesController(EBookDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Favorite>>> GetUserFavorites(int userId)
        {
            return await _context.Favorites
                .Include(f => f.Book)
                    .ThenInclude(b => b.Category)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Favorite>> PostFavorite(Favorite favorite)
        {
            // Check if already exists
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == favorite.UserId && f.BookId == favorite.BookId);

            if (existingFavorite != null)
            {
                return Conflict("Book is already in favorites");
            }

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserFavorites", new { userId = favorite.UserId }, favorite);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null)
            {
                return NotFound();
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("user/{userId}/book/{bookId}")]
        public async Task<IActionResult> RemoveFromFavorites(int userId, int bookId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (favorite == null)
            {
                return NotFound();
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}