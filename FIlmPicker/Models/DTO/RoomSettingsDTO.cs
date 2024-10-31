#nullable disable

namespace FIlmPicker.Models.DTO
{
    public class RoomSettingsDTO
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public float MinKpRaitings { get; set; }
        public float MaxKpRaitings { get; set; }
        public int MinYear { get; set; }
        public int MaxYear { get; set; }
        public int TypeNumber { get; set; }
        public IEnumerable<GenreDTO> Genres { get; set; }
    }
}
