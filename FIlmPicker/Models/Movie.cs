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

        public Movie(int id, string roomId, string name, string description, int typeNumber,
            int movieLength, int year, double kpRaiting, double imdbRaiting, string poster,
            UserScore ownerScore, UserScore guestScore, List<Genre> genres)
        {
            _id = id;
            _roomId = roomId;
            _name = name;
            _description = description;
            _typeNumber = typeNumber;
            _movieLength = movieLength;
            _year = year;
            _kpRaiting = kpRaiting;
            _imdbRaiting = imdbRaiting;
            _poster = poster;
            _ownerScore = ownerScore;
            _guestScore = guestScore;
            _genres = genres;
        }

        private Movie(int id, string name, string description, int typeNumber,
            int movieLength, int year, double kpRaiting, double imdbRaiting,
            string poster, List<Genre> genres)
        {
            _id = id;
            _roomId = string.Empty;
            _name = name;
            _description = description;
            _typeNumber = typeNumber;
            _movieLength = movieLength;
            _year = year;
            _kpRaiting = kpRaiting;
            _imdbRaiting = imdbRaiting;
            _poster = poster;
            _ownerScore = UserScore.None;
            _guestScore = UserScore.None;
            _genres = genres;
        }

        public static Movie CreateMovie(int id, string name, string description, int typeNumber,
            int movieLength, int year, double kpRaiting, double imdbRaiting,
            string poster, List<Genre> genres)
        {
            return new Movie(id, name, description, typeNumber,
                movieLength, year, kpRaiting, imdbRaiting, poster, genres);
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
