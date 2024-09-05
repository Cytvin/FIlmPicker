using FIlmPicker.KinopoiskAPI;

namespace FIlmPicker.Models
{
    public class RoomViewModel
    {
        public string RoomId { get; set; }
        public string OwnerUserName { get; set; }
        public MovieAPIModel Movie { get; set; }
    }
}
