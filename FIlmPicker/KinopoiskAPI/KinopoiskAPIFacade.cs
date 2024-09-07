using Microsoft.AspNetCore.Http.Extensions;
using System.Text.Json;
using FIlmPicker.Data.Models;

namespace FIlmPicker.KinopoiskAPI
{
    public class KinopoiskAPIFacade
    {
        private readonly string _apiKey;

        public KinopoiskAPIFacade(string APIKey)
        {
            _apiKey = APIKey;
        }

        public async Task<MovieAPIModel> GetRandomMovie(RoomSettings settings)
        {
            QueryBuilder queryBuilder = new QueryBuilder();
            queryBuilder.Add("rating.kp", $"{settings.MinKpRating}-{settings.MaxKpRating}");
            queryBuilder.Add("year", $"{settings.MinYear}-{settings.MaxYear}");
            queryBuilder.Add("typeNumber", settings.TypeNumber.ToString());

            Uri uri = new Uri($"https://api.kinopoisk.dev/v1.4/movie/random{queryBuilder.ToString()}");

            using (HttpClient http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                http.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
                HttpResponseMessage response = await http.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    HttpContent content = response.Content;

                    var movie = await content.ReadFromJsonAsync<MovieAPIModel>();

                    if (movie == null)
                    {
                        throw new JsonException("API return null data");
                    }

                    return movie;
                }
                else
                {
                    throw new BadHttpRequestException("API request was unsuccess", (int)response.StatusCode);
                }
            }
        }

        public async Task<MovieAPIModel> GetMovieById(int id)
        {
            Uri uri = new Uri($"https://api.kinopoisk.dev/v1.4/movie/{id}");

            using (HttpClient http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                http.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
                HttpResponseMessage response = await http.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    HttpContent content = response.Content;

                    var movie = await content.ReadFromJsonAsync<MovieAPIModel>();

                    return movie;
                }
                else
                {
                    throw new BadHttpRequestException($"API return {response.StatusCode} code");
                }
            }
        }
    }
}
