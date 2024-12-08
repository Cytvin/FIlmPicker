namespace FIlmPicker.Models
{
    public class MatchesViewModel
    {
        public Room Room { get; set; }
        public string SecondUserName { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
    }
}
