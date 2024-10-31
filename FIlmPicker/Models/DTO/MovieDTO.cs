#nullable disable

namespace FIlmPicker.Models.DTO
{
    public class MovieDTO
    {
        public int Id { get; set; }
        public string RoomId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int TypeNumber { get; set; }
        public int MovieLength { get; set; }
        public int Year { get; set; }
        public double KpRaiting { get; set; }
        public double ImdbRaiting { get; set; }
        public string Poster { get; set; }
        public int OwnerScore { get; set; }
        public int GuestScore { get; set; }
        public List<GenreDTO> Genres { get; set; }
    }
}
