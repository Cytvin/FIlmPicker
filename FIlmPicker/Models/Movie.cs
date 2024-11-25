using FIlmPicker.Models.DTO;

namespace FIlmPicker.Models
{
    public class Movie
    {
        private int _id;
        private string _roomId;
        private string _name;
        private string _description;
        private int _typeNumber;
        private int _movieLength;
        private int _year;
        private double _kpRaiting;
        private double _imdbRaiting;
        private string _poster;
        private UserScore _ownerScore;
        private UserScore _guestScore;
        private List<Genre> _genres;

        public int Id => _id;
        public string RoomId => _roomId;
        public string Name => _name;
        public string Description => _description;
        public int TypeNumber => _typeNumber;
        public int MovieLength => _movieLength;
        public int Year => _year;
        public double KpRaiting => _kpRaiting;
        public double ImdbRaiting => _imdbRaiting;
        public UserScore OwnerScore => _ownerScore;
        public UserScore GuestScore => _guestScore;
        public string Poster => _poster;
        public IEnumerable<Genre> Genres => _genres;

        public Movie(MovieDTO movie)
        {
            _id = movie.Id;
            _roomId = movie.RoomId;
            _name = movie.Name;
            _description = movie.Description;
            _typeNumber = movie.TypeNumber;
            _movieLength = movie.MovieLength;
            _year = movie.Year;
            _kpRaiting = Math.Round(movie.KpRaiting, 1);
            _imdbRaiting = movie.ImdbRaiting;
            _ownerScore = (UserScore)movie.OwnerScore;
            _guestScore = (UserScore)movie.GuestScore;
            _poster = movie.Poster;
            _genres = movie.Genres.Select(g => new Genre(g)).ToList();
        }

        public void SetOwnerScore(UserScore ownerScore)
        {
            _ownerScore = ownerScore;
        }

        public void SetGuestScore(UserScore guestScore)
        {
            _guestScore = guestScore;
        }
    }
}
