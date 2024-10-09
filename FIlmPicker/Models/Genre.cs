using FIlmPicker.Models.DTO;

namespace FIlmPicker.Models
{
    public class Genre
    {
        private string? _id;
        private string _name;

        public string? Id => _id;
        public string Name => _name;

        public Genre(string name)
        {
            _name = name;
        }

        public Genre(GenreDTO genre)
        {
            _id = genre.Id;
            _name = genre.Name;
        }
    }
}
