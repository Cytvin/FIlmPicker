using FIlmPicker.Models;
using FIlmPicker.Data;
using FIlmPicker.Data.Models;
using Microsoft.EntityFrameworkCore;
using FIlmPicker.Models.Converters;

namespace FIlmPicker.Services.DatabaseServices
{
    public class RoomService
    {
        private readonly ILogger<RoomService> _logger;
        private readonly ApplicationDbContext _context;

        public RoomService(ILogger<RoomService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Room?> GetRoomAsync(string id)
        {
            RoomEntity? roomEntity;

            roomEntity = await _context.Rooms
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (roomEntity == null)
            {
                return null;
            }

            return roomEntity.ToModel();
        }

        public async Task CreateRoomAsync(string ownerId, string guestId)
        {
            RoomEntity roomRecord = new RoomEntity
            {
                OwnerId = ownerId,
                GuestId = guestId,
                InviteAccepted = false
            };

            RoomSettingsEntity roomSettings = new RoomSettingsEntity()
            {
                MinKpRating = 1,
                MaxKpRating = 10,
                MinYear = 1990,
                MaxYear = 2100,
                MoviesReceived = 0,
                TypeNumber = 1,
                MinVotes = 5000
            };

            roomRecord.RoomSetting = roomSettings;

            _context.Rooms.Add(roomRecord);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoomAsync(Room room)
        {
            RoomEntity? roomEntity = await _context.Rooms.FindAsync(room.Id);

            if (roomEntity == null)
            {
                return;
            }

            roomEntity.InviteAccepted = room.InviteAccepted;
            roomEntity.OwnerIsOut = room.OwnerIsOut;
            roomEntity.GuestIsOut = room.GuestIsOut;

            _context.Rooms.Update(roomEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoomAsync(string id)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return;
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }

        public async Task AddMovieListToRoomAsync(string roomId, IEnumerable<Movie> movieList)
        {
            RoomEntity? room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return;
            }

            List<RoomMovieEntity> moviesForRoom = new List<RoomMovieEntity>();

            foreach (Movie movie in movieList)
            {
                RoomMovieEntity roomMovie = new RoomMovieEntity
                {
                    MovieId = movie.Id,
                    RoomId = roomId,
                    OwnerScore = 0,
                    GuestScore = 0
                };

                moviesForRoom.Add(roomMovie);
            }

            _context.RoomMovies.AddRange(moviesForRoom);
            await _context.SaveChangesAsync();
        }

        public async Task<Room?> IsRoomWithUsersExistAsync(string ownerId, string guestId)
        {
            RoomEntity? room;

            room = await _context.Rooms
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.GuestId == guestId);

            if (room == null)
            {
                return null;
            }

            return room.ToModel();
        }

        public async Task<IEnumerable<Room>> GetUserOwnRoomsAsync(string userId)
        {
            IEnumerable<RoomEntity> userOwnRooms = await _context.Rooms
                .Where(r => r.OwnerId == userId && r.OwnerIsOut == false)
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .ToListAsync();

            return userOwnRooms.Select(r => r.ToModel());
        }

        public async Task<IEnumerable<Room>> GetUserGuestRoomsAsync(string userId)
        {
            IEnumerable<RoomEntity> userGuestRooms = await _context.Rooms
                .Where(r => r.GuestId == userId && r.InviteAccepted && r.GuestIsOut == false)
                .Include(r => r.RoomSetting)
                .ThenInclude(rs => rs.Genres)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .ToListAsync();

            return userGuestRooms.Select(r => r.ToModel());
        }

        public async Task<IEnumerable<Room>> GetInvitationsAsync(string userId)
        {
            IEnumerable<RoomEntity> rooms = await _context.Rooms
                .Where(r => r.GuestId == userId && !r.InviteAccepted)
                .Include(r => r.Owner)
                .Include(r => r.Guest)
                .Include(r => r.RoomSetting)
                .ToListAsync();

            return rooms.Select(r => r.ToModel());
        }

        public async Task<int> GetInviteCountAsync(string userId)
        {
            int inviteCount = await _context.Rooms
                .Where(r => r.GuestId == userId && r.InviteAccepted == false)
                .CountAsync();

            return inviteCount;
        }
    }
}
