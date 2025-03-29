namespace FIlmPicker.Models
{
    public class Genre
    {
        private string _id;
        private string _name;

        public string? Id => _id;
        public string Name => _name;

        public Genre(string name)
        {
            _id = Guid.NewGuid().ToString();
            _name = name;
        }

        public Genre(string id, string name) : this(name)
        {
            _id = id;
        }
    }
}
