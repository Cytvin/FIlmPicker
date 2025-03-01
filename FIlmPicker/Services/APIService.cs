using KinopoiskAPI;
using KinopoiskAPI.Models;
using System.Text.Json;

namespace FIlmPicker.Services
{
    public class APIService
    {
        private readonly KinopoiskAPIClient _client;

        public APIService(string apiKey)
        {
            _client = new KinopoiskAPIClient(apiKey);
        }

        public async Task<IEnumerable<FIlmPicker.Models.Movie>> GetMovieByFilter(QueryString filter)
        {
            IEnumerable<Movie> movieList;

            try
            {
                movieList = await _client.GetMovieByFilter(filter.ToString());
            }
            catch (JsonException)
            {
                throw new JsonException("API return null data");
            }
            catch (BadHttpRequestException)
            {
                throw new HttpRequestException("API request was unsuccess");
            }

            return Convert(movieList);
        }

        public async Task<FIlmPicker.Models.Movie> GetMovieByIdAsync(int id)
        {
            Movie movie = await _client.GetMovieById(id);

            return Convert(movie);
        }

        private IEnumerable<FIlmPicker.Models.Movie> Convert(IEnumerable<Movie> movieList)
        {
            return movieList.Select(m => Convert(m));
        }

        private FIlmPicker.Models.Movie Convert(Movie movie)
        {
            List<FIlmPicker.Models.Genre> genres = movie.Genres.
                Select(g => new FIlmPicker.Models.Genre(g.Name)).ToList();


            return FIlmPicker.Models.Movie.CreateMovie(movie.Id, movie.Name, movie.Description,
                movie.TypeNumber, movie.MovieLength ?? movie.SeriesLength ?? 0, movie.Year,
                movie.Rating.Kp, movie.Rating.Imdb, movie.Poster.Url ?? "", genres);
        }
    }
}
