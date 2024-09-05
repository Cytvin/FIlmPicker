namespace FIlmPicker.KinopoiskAPI
{
    public class MovieAPIModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TypeNumber { get; set; }
        public int? MovieLength { get; set; }
        public int? SeriesLength { get; set; }
        public int? Year { get; set; }
        public string AlternativeName { get; set; }
        public RatingAPIModel Rating { get; set; }
        public List<GenreAPIModel> Genres { get; set; }
        public PosterAPIModel Poster { get; set; }
    }
}