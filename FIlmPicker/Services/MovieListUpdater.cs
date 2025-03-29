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

        public async Task Update(RoomSettings roomSettings)
        {
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async (ct, db) => 
            {
                await UpdateMoiveList(roomSettings, db);
            });

            _logger.LogInformation("Movie list update job added to queue. RoomId = {roomid}", roomSettings.RoomId);
        }

        private async Task UpdateMoiveList(RoomSettings roomSettings, DatabaseService databaseService)
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
                return;
            }
            catch (BadHttpRequestException)
            {
                _logger.LogInformation("API return bad request error");
                return;
            }

            int moviesReceived = movieList.Count();

            _logger.LogInformation("Received {count} movies", moviesReceived);

            roomSettings.SetMoviesReceived(moviesReceived);

            await databaseService.RoomSettingsService.UpdateRoomSettingsAsync(roomSettings);
            await databaseService.MovieService.SaveMovieListAsync(movieList);
            await databaseService.RoomService.AddMovieListToRoomAsync(roomSettings.RoomId, movieList);
            await databaseService.MovieListUpdaterQueueService.DeleteMovieListFromQueue(roomSettings.RoomId);

            foreach (Movie movie in movieList)
            {
                _logger.LogInformation("Movie from API: {Id}; {Name}; {TypeNumber}; {MovieLength}",
                    movie.Id, movie.Name, movie.TypeNumber, movie.MovieLength);
            }

            _logger.LogInformation("Movie list updated. Room ID: {roomid}", roomSettings.RoomId);
        }
    }
}