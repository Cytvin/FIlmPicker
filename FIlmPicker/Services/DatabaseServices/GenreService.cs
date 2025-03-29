using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using FIlmPicker.Models.Converters;
using FIlmPicker.Models;
using Microsoft.EntityFrameworkCore;

namespace FIlmPicker.Services.DatabaseServices
{
    public class GenreService
    {
        private readonly ILogger<GenreService> _logger;
        private readonly ApplicationDbContext _context;

        public GenreService(ILogger<GenreService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IEnumerable<Genre>> GetAllGenresAsync()
        {
            List<GenreEntity> genres = await _context.Genres.ToListAsync();

            return genres.Select(g => g.ToModel());
        }

        public async Task<Genre?> GetGenreByIdAsync(string id)
        {
            GenreEntity? genre = await _context.Genres.FindAsync(id);

            if (genre == null)
            {
                return null;
            }

            return genre.ToModel();
        }

        public async Task<GenreEntity> GetGenresRecordsByNameAsync(string name)
        {
            GenreEntity? genre = await _context.Genres
                .FirstOrDefaultAsync(x => x.Name == name);

            if (genre != null)
            {
                return genre;
            }

            genre = new GenreEntity()
            {
                Name = name
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return genre;
        }
    }
}
