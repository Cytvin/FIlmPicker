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

        public async Task<IEnumerable<Models.Movie>> GetMovieByFilter(QueryString filter)
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
            catch (HttpRequestException)
            {
                throw new HttpRequestException("API request was unsuccess");
            }

            return Convert(movieList);
        }

        private IEnumerable<Models.Movie> Convert(IEnumerable<Movie> movieList)
        {
            return movieList.Select(m => Convert(m));
        }

        private Models.Movie Convert(Movie movie)
        {
            List<Models.Genre> genres = movie.Genres
                .Select(g => new Models.Genre(g.Name))
                .ToList();

            return Models.Movie.CreateMovie(movie.Id, movie.Name, movie.Description,
                movie.TypeNumber, movie.MovieLength ?? movie.SeriesLength ?? 0, movie.Year,
                movie.Rating.Kp, movie.Rating.Imdb, movie.Poster.Url ?? "", genres);
        }
    }
}
