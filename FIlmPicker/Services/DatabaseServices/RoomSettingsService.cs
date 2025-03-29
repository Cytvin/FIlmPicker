using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using FIlmPicker.Models;
using Microsoft.EntityFrameworkCore;
using FIlmPicker.Models.Converters;

namespace FIlmPicker.Services.DatabaseServices
{
    public class RoomSettingsService
    {
        private readonly ILogger<RoomSettingsService> _logger;
        private readonly ApplicationDbContext _context;

        public RoomSettingsService(ILogger<RoomSettingsService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<RoomSettings?> GetRoomSettingsByIdAsync(string settingsId)
        {
            RoomSettingsEntity? roomSettings = await _context.RoomSettings
                .Include(rsg => rsg.Genres)
                .FirstOrDefaultAsync(rs => rs.Id == settingsId);

            if (roomSettings == null)
            {
                return null;
            }

            return roomSettings.ToModel();
        }

        public async Task UpdateRoomSettingsAsync(RoomSettings roomSettings)
        {
            RoomSettingsEntity? settings = await _context.RoomSettings
                .Include(rs => rs.Genres)
                .FirstOrDefaultAsync(rs => rs.Id == roomSettings.Id);

            if (settings == null)
            {
                return;
            }

            List<GenreEntity> genres = new List<GenreEntity>();

            foreach (var genreDTO in roomSettings.Genres)
            {
                GenreEntity? genre = await _context.Genres.FindAsync(genreDTO.Id);

                if (genre == null)
                {
                    continue;
                }

                genres.Add(genre);
            }

            settings.RoomId = roomSettings.RoomId;
            settings.MinKpRating = roomSettings.MinKpRating;
            settings.MaxKpRating = roomSettings.MaxKpRating;
            settings.MinYear = roomSettings.MinYear;
            settings.MaxYear = roomSettings.MaxYear;
            settings.TypeNumber = roomSettings.TypeNumber;
            settings.MoviesReceived = roomSettings.MoviesReceived;
            settings.Genres = genres;

            _context.RoomSettings.Update(settings);
            await _context.SaveChangesAsync();
        }
    }
}
