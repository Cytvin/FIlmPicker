namespace FIlmPicker.Services.DatabaseServices
{
    public class DatabaseService
    {
        private readonly RoomService _roomService;
        private readonly MovieService _movieService;
        private readonly UserService _userService;
        private readonly GenreService _genreService;
        private readonly RoomSettingsService _roomSettingsService;
        private readonly MovieListUpdaterQueueService _movieListUpdaterQueueService;

        public RoomService RoomService => _roomService;
        public MovieService MovieService => _movieService;
        public UserService UserService => _userService;
        public GenreService GenreService => _genreService;
        public RoomSettingsService RoomSettingsService => _roomSettingsService;
        public MovieListUpdaterQueueService MovieListUpdaterQueueService => _movieListUpdaterQueueService;

        public DatabaseService(RoomService roomService, UserService userService, GenreService genreService,
            MovieService movieService, RoomSettingsService roomSettingsService, MovieListUpdaterQueueService movieListUpdaterQueueService)
        {
            _roomService = roomService;
            _userService = userService;
            _genreService = genreService;
            _movieService = movieService;
            _roomSettingsService = roomSettingsService;
            _movieListUpdaterQueueService = movieListUpdaterQueueService;
        }
    }
}