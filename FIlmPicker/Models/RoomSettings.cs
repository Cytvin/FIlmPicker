using FIlmPicker.Models.DTO;
using Microsoft.AspNetCore.Http.Extensions;

namespace FIlmPicker.Models
{
    public class RoomSettings
    {
        private string _id;
        private string _roomId;
        private float _minKpRating;
        private float _maxKpRating;
        private int _minYear;
        private int _maxYear;
        private int _typeNumber;
        private List<Genre> _genres;
        private List<Movie> _movieInRoom;
        private int _minVotes;

        public string Id => _id;
        public string RoomId => _roomId;
        public float MinKpRating => _minKpRating;
        public float MaxKpRating => _maxKpRating;
        public int MinYear => _minYear;
        public int MaxYear => _maxYear;
        public int TypeNumber => _typeNumber;
        public int MinVotes => _minVotes;
        public IEnumerable<Genre> Genres => _genres;

        public RoomSettings(RoomSettingsDTO roomSettings)
        {
            _id = roomSettings.Id;
            _roomId = roomSettings.RoomId;
            _minKpRating = roomSettings.MinKpRaitings;
            _maxKpRating = roomSettings.MaxKpRaitings;
            _minYear = roomSettings.MinYear;
            _maxYear = roomSettings.MaxYear;
            _typeNumber = roomSettings.TypeNumber;
            _minVotes = 10000;
            _movieInRoom = new List<Movie>();
            _genres = roomSettings.Genres.Select(g => new Genre(g)).ToList();
        }

        public RoomSettings(string roomId)
        {
            _roomId = roomId;
            _minKpRating = 1;
            _maxKpRating = 10;
            _minYear = 1900;
            _maxYear = 2024;
            _typeNumber = 1;
            _minVotes = 10000;
            _movieInRoom = new List<Movie>();
            _genres = new List<Genre>();
        }

        public void SetMinKpRating(float minKpRaiting)
        {
            if (minKpRaiting < 0 || minKpRaiting > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(minKpRaiting), "minKpRaiting can't be less than 0 and greater than 10");
            }

            _minKpRating = minKpRaiting;
        }

        public void SetMaxKpRating(float maxKpRaiting)
        {
            if (maxKpRaiting < 0 || maxKpRaiting > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(maxKpRaiting), "minKpRaiting can't be less than 0 and greater than 10");
            }

            _maxKpRating = maxKpRaiting;
        }

        public void SetMinYear(int minYear)
        {
            _minYear = minYear;
        }

        public void SetMaxYear(int maxYear)
        {
            _maxYear = maxYear;
        }

        public void SetTypeNumber(int typeNumber)
        {
            if (typeNumber < 1 || typeNumber > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(typeNumber), "typeNumber can't be less than 1 and greater than 5");
            }

            _typeNumber = typeNumber;
        }

        public void SetMovieInRoom(List<Movie> movieInRoom)
        {
            _movieInRoom = movieInRoom;
        }

        public void AddGenre(Genre genre)
        {
            _genres.Add(genre);
        }

        public void RemoveAllGenre()
        {
            _genres.Clear();
        }

        public QueryString GetQueryString()
        {
            QueryBuilder queryBuilder = new QueryBuilder
            {
                { "rating.kp", $"{_minKpRating}-{_maxKpRating}" },
                { "year", $"{_minYear}-{_maxYear}" },
                { "typeNumber", _typeNumber.ToString() },
                { "votes.kp", $"{_minVotes}-999999999"}
            };

            foreach (Genre genre in _genres)
            {
                queryBuilder.Add("genres.name", genre.Name);
            }

            foreach (Movie item in _movieInRoom)
            {
                queryBuilder.Add("id", $"!{item.Id}");
            }

            return queryBuilder.ToQueryString();
        }
    }
}
