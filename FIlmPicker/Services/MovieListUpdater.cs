using FIlmPicker.Models;
using FIlmPicker.Models.DTO;
using System.Net;
using System.Text.Json;

namespace FIlmPicker.Services
{
    public class MovieListUpdater
    {
        private ILogger<MovieListUpdater> _logger;
        private readonly DatabaseService _dbService;
        private readonly APIService _APIService;

        public MovieListUpdater(ILogger<MovieListUpdater> logger, DatabaseService dbService, APIService APIService) 
        {
            _logger = logger;
            _dbService = dbService;
            _APIService = APIService;
        }

        public async Task<HttpStatusCode> Update(RoomSettings roomSettings)
        {
            IEnumerable<MovieDTO> moviesInRoomDTO = await _dbService.GetMoviesInRoomAsync(roomSettings.RoomId);
            List<Movie> moviesInRoom = moviesInRoomDTO.Select(m => new Movie(m)).ToList();
            roomSettings.SetMovieInRoom(moviesInRoom);

            IEnumerable<MovieDTO> movieList = new List<MovieDTO>();

            try
            {
                movieList = await _APIService.GetMovieByFilter(roomSettings.GetQueryString());
            }
            catch (JsonException)
            {
                return HttpStatusCode.NotFound;
            }
            catch (InvalidOperationException)
            {
                return HttpStatusCode.NotFound;
            }
            catch (BadHttpRequestException)
            {
                return HttpStatusCode.BadRequest;
            }

            _logger.LogInformation("Received {count} movies", movieList.Count());

            await _dbService.SaveMovieListAsync(movieList);
            await _dbService.AddMovieListToRoomAsync(roomSettings.RoomId, movieList);

            foreach (MovieDTO movie in movieList)
            {
                _logger.LogInformation("Movie from API: {Id}; {Name}; {TypeNumber}; {MovieLength}",
                    movie.Id, movie.Name, movie.TypeNumber, movie.MovieLength);
            }

            return HttpStatusCode.OK;
        }
    }
}