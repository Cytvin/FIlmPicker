namespace FIlmPicker.Models
{
    public class RoomsViewModel
    {
        public IEnumerable<Room> OwnerRooms { get; set; }
        public IEnumerable<Room> GuestRooms { get; set; }
        public int UnacceptedInviteCount { get; set; }
    }
}
