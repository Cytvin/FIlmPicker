#nullable disable

namespace FIlmPicker.Models
{
    public class RoomsViewModel
    {
        public IEnumerable<Room> Rooms { get; set; }
        public int UnacceptedInviteCount { get; set; }
    }
}
