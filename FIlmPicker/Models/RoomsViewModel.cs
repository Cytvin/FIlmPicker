namespace FIlmPicker.Models
{
    public class RoomsViewModel
    {
        public IEnumerable<Room> Rooms { get; set; }
        public StatusMessage? StatusMessage { get; set; }
    }
}
