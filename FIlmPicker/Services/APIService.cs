using KinopoiskAPI;
using FIlmPicker.Models.DTO;
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

        public async Task<List<MovieDTO>> GetMovieByFilter(QueryString filter)
        {
            List<Movie> movieList;

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

        public async Task<MovieDTO> GetMovieByIdAsync(int id)
        {
            Movie movie = await _client.GetMovieById(id);

            return Convert(movie);
        }

        private List<MovieDTO> Convert(List<Movie> movieList)
        {
            return movieList.Select(m => Convert(m)).ToList();
        }

        private MovieDTO Convert(Movie movie)
        {
            MovieDTO result = new MovieDTO()
            {
                Id = movie.Id,
                Name = movie.Name,
                Description = movie.Description,
                TypeNumber = movie.TypeNumber,
                MovieLength = movie.MovieLength ?? movie.SeriesLength ?? 0,
                Year = movie.Year ?? 0,
                KpRaiting = movie.Rating.Kp,
                ImdbRaiting = movie.Rating.Imdb,
                OwnerScore = 0,
                GuestScore = 0,
                Poster = movie.Poster.Url ?? "",
                Genres = movie.Genres.Select(g => new GenreDTO() { Name = g.Name }).ToList()
            };

            return result;
        }
    }
}
