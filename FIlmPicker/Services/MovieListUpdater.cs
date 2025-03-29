using FIlmPicker.Models;
using FIlmPicker.Services.DatabaseServices;
using System.Text.Json;

namespace FIlmPicker.Services
{
    public class MovieListUpdater
    {
        private readonly ILogger<MovieListUpdater> _logger;
        private readonly DatabaseService _dbService;
        private readonly APIService _APIService;
        private readonly BackgroundTaskQueue _backgroundTaskQueue;

        public MovieListUpdater(ILogger<MovieListUpdater> logger, DatabaseService dbService, APIService APIService, BackgroundTaskQueue backgroundTaskQueue) 
        {
            _logger = logger;
            _dbService = dbService;
            _APIService = APIService;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public async Task InitializeQueue()
        {
            _logger.LogInformation("Start queue initialization");

            IEnumerable<RoomSettings> roomSettings = await _dbService.MovieListUpdaterQueueService.GetAllRoomsSettingsForUpdate();

            _logger.LogInformation("Loaded {roomsCount} rooms for update", roomSettings.Count());

            foreach (RoomSettings rs in roomSettings)
            {
                await Update(rs);
            }
        }

        public async Task Update(RoomSettings roomSettings)
        {
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (ct, db) => 
            {
                return await UpdateMoiveList(roomSettings, db);
            });

            _logger.LogInformation("Movie list update job added to queue. RoomId = {roomid}", roomSettings.RoomId);
        }

        private async Task<bool> UpdateMoiveList(RoomSettings roomSettings, DatabaseService databaseService)
        {
            _logger.LogInformation("Start update movie list for room {roomId}", roomSettings.RoomId);

            IEnumerable<Movie> moviesInRoom = await databaseService.MovieService.GetMoviesInRoomAsync(roomSettings.RoomId);
            roomSettings.SetMovieInRoom(moviesInRoom.ToList());

            IEnumerable<Movie> movieList = new List<Movie>();

            try
            {
                movieList = await _APIService.GetMovieByFilter(roomSettings.GetQueryString());
            }
            catch (JsonException)
            {
                _logger.LogInformation("Json deserialize error");
                return false;
            }
            catch (HttpRequestException)
            {
                _logger.LogInformation("API return bad request error");
                return false;
            }

            int moviesReceived = movieList.Count();

            _logger.LogInformation("Received {count} movies", moviesReceived);

            roomSettings.SetMoviesReceived(moviesReceived);

            await databaseService.RoomSettingsService.UpdateRoomSettingsAsync(roomSettings);
            await databaseService.MovieService.SaveMovieListAsync(movieList);
            await databaseService.RoomService.AddMovieListToRoomAsync(roomSettings.RoomId, movieList);
            await databaseService.MovieListUpdaterQueueService.DeleteMovieListFromQueue(roomSettings.RoomId);

            _logger.LogInformation("Movie list updated. Room ID: {roomid}", roomSettings.RoomId);

            return true;
        }
    }
}