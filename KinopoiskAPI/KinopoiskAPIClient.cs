using System.Net.Http.Json;
using System.Text.Json;
using KinopoiskAPI.Models;

namespace KinopoiskAPI
{
    public class KinopoiskAPIClient
    {
        private readonly string _apiKey;

        public KinopoiskAPIClient(string APIKey)
        {
            _apiKey = APIKey;
        }

        public async Task<Movie> GetRandomMovie(string? queryParameters = null)
        {
            queryParameters ??= "";

            Uri uri = new Uri($"https://api.kinopoisk.dev/v1.4/movie/random{queryParameters}&notNullFields=poster.url&notNullFields=name");

            Console.WriteLine(uri.ToString());

            using (HttpClient http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                http.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
                HttpResponseMessage response = await http.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    HttpContent content = response.Content;

                    var movie = await content.ReadFromJsonAsync<Movie>();

                    if (movie == null)
                    {
                        throw new JsonException("API return null data");
                    }

                    return movie;
                }
                else
                {
                    throw new HttpRequestException("API request was unsuccess", null, response.StatusCode);
                }
            }
        }

        public async Task<Movie> GetMovieById(int id)
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

                    var movie = await content.ReadFromJsonAsync<Movie>();

                    if (movie == null)
                    {
                        throw new JsonException("API return null data");
                    }

                    return movie;
                }
                else
                {
                    throw new HttpRequestException("API request was unsuccess", null, response.StatusCode);
                }
            }
        }
    }
}
