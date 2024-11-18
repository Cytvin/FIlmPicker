#nullable disable

namespace KinopoiskAPI.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TypeNumber { get; set; }
        public int? MovieLength { get; set; }
        public int? SeriesLength { get; set; }
        public int? Year { get; set; }
        public string AlternativeName { get; set; }
        public Rating Rating { get; set; }
        public List<Genre> Genres { get; set; }
        public Poster Poster { get; set; }
    }
}