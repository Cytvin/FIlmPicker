using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FIlmPicker.Services.DatabaseServices
{
    public class MovieListUpdaterQueueService
    {
        private readonly ILogger<MovieListUpdaterQueueService> _logger;
        private readonly ApplicationDbContext _context;

        public MovieListUpdaterQueueService(ILogger<MovieListUpdaterQueueService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task AddMovieListToQueue(string roomId)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return;
            }

            MovieListUpdaterQueue movieListOnUpdate = new MovieListUpdaterQueue()
            {
                RoomId = roomId
            };

            _context.MovieListsOnUpdate.Add(movieListOnUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsMovieListInQueue(string roomId)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return false;
            }

            return await _context.MovieListsOnUpdate.AnyAsync(ml => ml.RoomId == room.Id);
        }

        public async Task DeleteMovieListFromQueue(string roomId)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return;
            }

            await _context.MovieListsOnUpdate.Where(ml => ml.RoomId == room.Id).ExecuteDeleteAsync();
        }
    }
}
