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

        public async Task<MovieDTO> GetRandomMovieAsync(QueryString queryString)
        {
            Movie movie;

            try
            {
                movie = await _client.GetMovieByFilter(queryString.ToString());
            }
            catch (JsonException ex)
            {
                throw new JsonException("API return null data");
            }
            catch (BadHttpRequestException ex)
            {
                throw new HttpRequestException("API request was unsuccess");
            }

            return Convert(movie);
        }

        public async Task<MovieDTO> GetMovieByIdAsync(int id)
        {
            Movie movie = await _client.GetMovieById(id);

            return Convert(movie);
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
