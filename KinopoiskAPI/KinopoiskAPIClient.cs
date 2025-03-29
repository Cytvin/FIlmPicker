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

        public async Task<IEnumerable<Movie>> GetMovieByFilter(string? queryParameters = null)
        {
            queryParameters ??= "";

            Uri uri = new Uri($"https://api.kinopoisk.dev/v1.4/movie{queryParameters}&notNullFields=poster.url&notNullFields=name&page=1&limit=250");

            using (HttpClient http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                http.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
                HttpResponseMessage response = await http.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    HttpContent content = response.Content;

                    var apiResponse = await content.ReadFromJsonAsync<ApiResponse>();
                    var contentString = await content.ReadAsStringAsync();

                    if (apiResponse == null)
                    {
                        throw new JsonException("API return null data");
                    }

                    return apiResponse.Docs;
                }
                else
                {
                    var contentString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(contentString);
                    throw new HttpRequestException("API request was unsuccess", null, response.StatusCode);
                }
            }
        }
    }
}
